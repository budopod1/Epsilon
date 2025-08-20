#include <stdlib.h>
#include <stdint.h>
#include <string.h>

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

void proc_exit(int32_t code) {
    exit((int)code);
}

struct Array *proc_get_argv() {
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
