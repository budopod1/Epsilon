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

#include "builtins.h"

#define ERR_START "FATAL ERROR IN proc: "

static struct Array *dup_C_str_to_epsl_str(uint64_t ref_counter, char *src) {
    uint64_t length = strlen(src);
    uint64_t capacity = length + 1;
    char *content = malloc(capacity);
    strcpy(content, src);
    struct Array *result = malloc(sizeof(*result));
    result->ref_counter = ref_counter;
    result->capacity = capacity;
    result->length = length;
    result->content = content;
    return result;
}

static struct Array *C_str_to_epsl_str(uint64_t ref_counter, char *src) {
    struct Array *result = malloc(sizeof(*result));
    result->ref_counter = ref_counter;
    uint64_t length = strlen(src);
    result->capacity = length + 1;
    result->length = length;
    result->content = (unsigned char*)src;
    return result;
}

#ifdef _WIN32
struct Array *wchar_str_to_epsl_str(uint64_t ref_counter, wchar_t *wstr) {
    int result_capacity = WideCharToMultiByte(
        CP_UTF8, // dest encoding
        MB_ERR_INVALID_CHARS, // flags
        wstr, // src str
        -1, // src len (-1 indicates NULL-termination)
        NULL, // dest buffer (ignored due to next param)
        0, // dest buffer size (0 indicated do not write, just calc size)
        NULL, NULL // unused arguments
    );
    if (result_capacity == 0) return NULL;

    char *result_content = epsl_malloc(result_capacity);
    int status = WideCharToMultiByte(
        CP_UTF8, // dest encoding
        MB_ERR_INVALID_CHARS, // flags
        wstr, // src str
        -1, // src len
        result_content, // dest buffer
        result_capacity, // dest buffer size
        NULL, NULL // unused arguments
    );
    if (status == 0) {
        free(result_content);
        return NULL;
    }

    struct Array *result = malloc(sizeof(*result));
    result->ref_counter = ref_counter;
    result->capacity = result_capacity;
    result->length = result_capacity - 1;
    result->content = result_content;

    return result;
}
#endif

void proc_exit(int32_t code) {
    exit((int)code);
}

struct Array *proc_get_argv(void) {
    if (epsl_argv == NULL) {
        epsl_panicf(ERR_START "argv is not available");
    }
    struct Array *arg_array = epsl_blank_array(sizeof(struct Array*));
    char **argv_ptr = epsl_argv;
    while (*argv_ptr) {
        struct Array **new_arg_ptr = ((struct Array**)arg_array->content) + arg_array->length;
        epsl_increment_length(arg_array, sizeof(struct Array*));
        *new_arg_ptr = dup_C_str_to_epsl_str(1, *argv_ptr);
        argv_ptr++;
    }
    return arg_array;
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
        return C_str_to_epsl_str(0, path);
    }
    path = epsl_realloc(path, path_size);
    if (_NSGetExecutablePath(path, &path_size) == 0) {
        return C_str_to_epsl_str(0, path);
    } else {
        epsl_panicf(ERR_START "Cannot determine executable path");
    }
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

    struct Array *result = wchar_str_to_epsl_str(0, wpath);
    if (result == NULL) {
        epsl_panicf(ERR_START "Executable path cannot be read as UTF-8");
    }
    return result;
#else
    epsl_panicf(ERR_START, "get executable path is not supported on this system")
#endif
}
