#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#include "epsilon.h"

#ifdef _WIN32
#include <windows.h>
#else
#include <dlfcn.h>
#endif

struct PolymorphicStruct {
    void *struct_;
    void *vtable;
};

typedef struct PolymorphicStruct (*dllib_func)(struct PolymorphicStruct);

struct DynamicLibrary {
    uint64_t ref_counter;
    void *handle;
};

struct LibraryFunction {
    uint64_t ref_counter;
    struct DynamicLibrary *lib;
    dllib_func addr;
};

struct LibraryGlobal {
    uint64_t ref_counter;
    struct DynamicLibrary *lib;
    struct PolymorphicStruct *addr;
};

struct DynamicLibrary *dllib_load_dl(struct Array *name) {
#ifdef _WIN32
    SetLastError(0);
    wchar_t *windows_name = epsl_Estr_to_Wstr(name);
    if (!windows_name) return NULL;
    void *handle = LoadLibraryW(windows_name);
    free(windows_name);
#else
    dlerror(); // clear any current error
    char *c_name = epsl_Estr_to_Cstr(name);
    void *handle = dlopen(c_name, RTLD_LAZY);
    free(c_name);
#endif

    if (!handle) return NULL;

    struct DynamicLibrary *lib = epsl_malloc(sizeof(*lib));
    lib->ref_counter = 0;
    lib->handle = handle;

    return lib;
}

struct Array *dllib_get_error_text(void) {
#ifdef _WIN32
    DWORD error = GetLastError();
    if (error == 0) {
        return NULL;
    }
    DWORD flags = FORMAT_MESSAGE_ALLOCATE_BUFFER
        | FORMAT_MESSAGE_FROM_SYSTEM
        | FORMAT_MESSAGE_IGNORE_INSERTS;
    wchar_t *buf;
    DWORD len = FormatMessageW(
        flags,
        NULL,         // source
        error,
        0,            // language ID
        (LPTSTR)&buf,
        0,            // minimum output buffer size
        NULL          // formatting arguments
    );
    if (len == 0) {
        epsl_panicf("Failed to obtain error message");
    }
    struct Array *result = epsl_Wstr_to_Estr(0, buf);
    LocalFree(buf);
    return result;
#else
    char *err = dlerror();
    if (err) {
        return epsl_dup_Cstr_to_Estr(0, err);
    } else {
        return NULL;
    }
#endif
}

static void *_get_lib_symbol(struct DynamicLibrary *lib, struct Array *name) {
    char *c_name = epsl_Estr_to_Cstr(name);
#ifdef _WIN32
    SetLastError(0);
    void *symbol = GetProcAddress(lib->handle, c_name);
#else
    dlerror();
    void *symbol = dlsym(lib->handle, c_name);
#endif
    free(c_name);
    return symbol;
}

struct LibraryFunction *dllib_get_function(struct DynamicLibrary *lib, struct Array *name) {
    void *addr = _get_lib_symbol(lib, name);
    if (!addr) return NULL;

    struct LibraryFunction *func = epsl_malloc(sizeof(*func));
    func->ref_counter = 0;
    func->lib = lib;
    lib->ref_counter++;
    func->addr = addr;

    return func;
}

struct PolymorphicStruct dllib_call_function(struct LibraryFunction *func, struct PolymorphicStruct arg) {
    uint64_t *ref_counter = (uint64_t*)arg.struct_;
    ++*ref_counter;
    struct PolymorphicStruct result = (*func->addr)(arg);
    --*ref_counter;
    return result;
}

struct LibraryGlobal *dllib_get_global(struct DynamicLibrary *lib, struct Array *name) {
    void *addr = _get_lib_symbol(lib, name);
    if (!addr) return NULL;

    struct LibraryGlobal *global = epsl_malloc(sizeof(*global));
    global->ref_counter = 0;
    global->lib = lib;
    lib->ref_counter++;
    global->addr = addr;

    return global;
}

struct PolymorphicStruct dllib_get_global_value(struct LibraryGlobal *global) {
    return *global->addr;
}

void dllib_set_global_value(struct LibraryGlobal *global, struct PolymorphicStruct value) {
    *global->addr = value;
}

void dllib_close_dl(struct DynamicLibrary *lib) {
#ifdef _WIN32
    FreeLibrary(lib->handle);
#else
    dlclose(lib->handle);
#endif
}
