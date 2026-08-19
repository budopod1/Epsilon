#define _POSIX_C_SOURCE 202405L

#include <stdlib.h>
#include <stdint.h>
#include <string.h>

#ifdef __linux__
#include <unistd.h>
#elif __APPLE__
#include <mach-o/dyld.h>
#elif _WIN32
#include <windows.h>
#include <libloaderapi.h>
#endif

#include "epsilon.h"

#define ERR_START "FATAL ERROR IN proc: "

void proc_exit(int32_t code) {
    exit((int)code);
}

struct Array *proc_get_argv(void) {
    if (epsl_argv == NULL) {
        epsl_panicf(ERR_START "argv is not available");
    }
    struct Array *arg_arr = epsl_blank_array(sizeof(struct Array*));
    char **argv_ptr = epsl_argv;
    while (*argv_ptr) {
        epsl_increment_length(arg_arr, sizeof(struct Array*));
        struct Array *arg_str = epsl_dup_Cstr_to_Estr(1, *argv_ptr);
        ((struct Array**)arg_arr->content)[arg_arr->length - 1] = arg_str;
        argv_ptr++;
    }
    return arg_arr;
}

struct Array *proc_get_executable_path(void) {
#ifdef __linux__
    char *exec_symlink = "/proc/self/exe";

    uint64_t path_cap = 1024;
    uint64_t path_len;
    char *path = NULL;
    while (1) {
        path = epsl_realloc(path, path_cap);
        ssize_t status = readlink(exec_symlink, path, path_cap);
        if (status == -1) {
            epsl_panicf(ERR_START "%s cannot be read", exec_symlink);
        } else if (status == path_cap) {
            path_cap *= 2;
            continue;
        } else {
            path_len = status;
            break;
        }
    }

    struct Array *result = malloc(sizeof(*result));
    result->ref_counter = 0;
    result->capacity = path_cap;
    result->length = path_len;
    result->content = path;
    return result;
#elif __APPLE__
    uint32_t path_size = 1024;
    char *path = epsl_malloc(path_size);
    if (_NSGetExecutablePath(path, &path_size) == 0) {
        return epsl_Cstr_to_Estr(0, path);
    }
    path = epsl_realloc(path, path_size);
    if (_NSGetExecutablePath(path, &path_size) != 0) {
        epsl_panicf(ERR_START "Cannot determine executable path");
    }
    return epsl_Cstr_to_Estr(0, path);
#elif _WIN32
    DWORD wpath_size = 1024;
    wchar_t *wpath = NULL;
    do {
        wpath = epsl_realloc(wpath, wpath_size * sizeof(wchar_t));
        DWORD written_len = GetModuleFileNameW(NULL, wpath, wpath_size);
        if (written_len == 0) {
            epsl_panicf(ERR_START "Cannot determine executable path");
        } else if (written_len >= wpath_size) {
            wpath_size *= 2;
            continue;
        }
    } while (0);
    struct Array *result = epsl_Wstr_to_Estr(0, wpath);
    if (result == NULL) {
        epsl_panicf(ERR_START "Executable path cannot be read as UTF-8");
    }
    return result;
#else
    epsl_panicf(ERR_START, "get executable path is not supported on this system")
#endif
}

struct Array *proc_get_env(struct Array *name) {
    char *c_name = epsl_Estr_to_Cstr(name);

    char *c_val = getenv(c_name);
    free(c_name);
    if (c_val == NULL) {
        return NULL;
    }

    return epsl_dup_Cstr_to_Estr(0, c_val);
}

bool proc_set_env(struct Array *name, struct Array *val) {
    char *c_name = epsl_Estr_to_Cstr(name);
    char *c_val = epsl_Estr_to_Cstr(val);

#ifdef _WIN32
    bool status = _putenv_s(c_name, c_val) == 0;
#else
    bool status = setenv(c_name, c_val, 1) == 0;
#endif

    free(c_name);
    free(c_val);

    return status;
}

bool proc_unset_env(struct Array *name) {
    char *c_name = epsl_Estr_to_Cstr(name);

#ifdef _WIN32
    bool status = _putenv_s(c_name, "") == 0;
#else
    bool status = unsetenv(c_name) == 0;
#endif

    free(c_name);

    return status;
}

int32_t proc_get_current_pid(void) {
#ifdef _WIN32
    return GetCurrentProcessId();
#else
    return getpid();
#endif
}
