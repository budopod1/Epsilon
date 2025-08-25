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
    void *val;
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

static char *c_str_from_epsl_str(struct Array *str) {
    char *result = epsl_malloc(str->length + 1);
    memcpy(result, str->content, str->length);
    result[str->length] = '\0';
    return result;
}

#ifdef _WIN32
static wchar_t *windows_str_from_epsl_str(struct Array *epsl_str) {
    int wstr_size = MultiByteToWideChar(
        CP_UTF8, // source encoding
        MB_ERR_INVALID_CHARS, // flags
        epsl_str->content, // src str
        epsl_str->length, // src len
        NULL, // dest buffer (ignored due to next param)
        0 // dest buffer size (0 indicated do not write, just calc size)
    );
    if (wstr_size == 0) return NULL;
    wchar_t *wstr = epsl_malloc(wstr_size * sizeof(wchar_t));
    int status = MultiByteToWideChar(
        CP_UTF8, // source encoding
        MB_ERR_INVALID_CHARS, // flags
        epsl_str->content, // src str
        epsl_str->length, // src len
        wstr, // dest buffer
        wstr_size // dest buffer size
    );
    if (status == 0) {
        free(wstr);
        return NULL;
    }
    return wstr;
}
#endif

struct DynamicLibrary *dllib_load_dl(struct Array *name) {
#ifdef _WIN32
    wchar_t *windows_name = windows_str_from_epsl_str(name);
    if (!windows_name) return NULL;
    void *handle = LoadLibraryW(windows_name);
    free(windows_name);
#else
    char *c_name = c_str_from_epsl_str(name);
    void *handle = dlopen(c_name, RTLD_LAZY);
    free(c_name);
#endif

    if (!handle) return NULL;

    struct DynamicLibrary *lib = epsl_malloc(sizeof(*lib));
    lib->ref_counter = 0;
    lib->handle = handle;

    return lib;
}

static void *_get_lib_symbol(struct DynamicLibrary *lib, struct Array *name) {
    char *c_name = c_str_from_epsl_str(name);
#ifdef _WIN32
    void *symbol = GetProcAddress(lib->handle, c_name);
#else
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
    uint64_t *ref_counter = (uint64_t*)arg.val;
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
