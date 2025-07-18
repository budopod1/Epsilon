#ifndef EPSL_BUILTINS_H

#define EPSL_BUILTINS_H

#include <stdint.h>
#include <stdbool.h>

// elem types are uint64_ts
// upper 62 bits are the size
// second to lowest bit is 1 when the value is not a pointer
// lowest bit is 1 when the value is nullable

#if __STDC_VERSION__ < 201112L
#error "C11 or greater is required"
#elif __STDC_VERSION__ < 202311L
#define thread_local _Thread_local
#endif

#ifndef __has_builtin
#define __has_builtin(x) 0
#endif

#if !__has_builtin(__builtin_expect)
#define __builtin_expect(expr, val) expr
#endif

struct Array {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    void *content;
};

struct VTableBase {
    uint64_t id_num;
    uint64_t extendee_count;
    uint64_t *extendees;
    struct Array *id_str;
    void (*free_fn)(void*);
};

#define ERROR_STACK_SIZE 4096

extern const char *epsl_error_stack[ERROR_STACK_SIZE];

extern const char **epsl_error_stack_top;

void epsl_panic(const char *message, uint64_t message_len);

void epsl_panicf(const char *format, ...);

int32_t epsl_memcmp(const void *lhs, const void *rhs, uint64_t count);

int32_t epsl_printf(const char *format, ...);

int32_t epsl_sprintf(char *buffer, const char *format, ...);

int32_t epsl_snprintf(char *buffer, uint64_t bufsz, const char *format, ...);

void epsl_exit(uint32_t status);

void epsl_out_of_memory_fail();

void *epsl_malloc(uint64_t size);

void *epsl_calloc(uint64_t num, uint64_t size);

void *epsl_realloc(void *ptr, uint64_t new_size);

void epsl_nonresizable_array_fail();

void epsl_increment_length(struct Array *array, uint64_t elem_size);

void epsl_require_capacity(struct Array *array, uint64_t required, uint64_t elem_size);

void epsl_increace_capacity(struct Array *array, uint64_t required, uint64_t elem_size);

void epsl_shrink_mem(struct Array *array, uint64_t elem_size);

void epsl_remove_at(struct Array *array, uint64_t idx, uint64_t elem_size);

void epsl_insert_space(struct Array *array, uint64_t idx, uint64_t elem_size);

void epsl_increment_array_ref_counts(const struct Array *array, uint64_t elem);

struct Array *epsl_clone_array(const struct Array *array, uint64_t elem);

void epsl_extend_array(struct Array *array1, const struct Array *array2, uint64_t elem);

struct Array *epsl_concat_arrays(const struct Array *array1, const struct Array *array2, uint64_t elem);

struct Array *epsl_blank_array(uint64_t elem_size);

void epsl_print(const struct Array *string);

void epsl_println(const struct Array *string);

extern inline char *epsl_format_W8();

extern inline char *epsl_format_W16();

extern inline char *epsl_format_W32();

extern inline char *epsl_format_W64();

extern inline char *epsl_format_Z8();

extern inline char *epsl_format_Z16();

extern inline char *epsl_format_Z32();

extern inline char *epsl_format_Z64();

struct Array *epsl_slice_array(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem);

struct Array *epsl_nest_array(const struct Array *arr, uint64_t elem);

struct Array *epsl_join_array(const struct Array *arr, const struct Array *sep, uint64_t elem);

struct Array *epsl_prefix_concat(const struct Array *arr, const struct Array *prefix, uint64_t elem);

struct Array *epsl_postfix_concat(const struct Array *arr, const struct Array *postfix, uint64_t elem);

uint64_t epsl_parse_int(const struct Array *str);

double epsl_parse_float(const struct Array *str);

struct Array *epsl_read_input_line();

void epsl_abort(const struct Array *string);

void epsl_abort_void();

struct Array *epsl_make_blank_array(uint64_t len, uint64_t elem_size);

void epsl_sort_array(struct Array *array, uint64_t elem_size, int32_t (*compar)(const void*, const void*));

struct Array *epsl_repeat_array(const struct Array *array, uint64_t times, uint64_t elem);

void epsl_null_value_fail();

struct Array *epsl_format_string(struct Array *template_, struct Array *values[], uint32_t value_count);

bool epsl_check_vtable_extends(struct VTableBase *vtable, uint64_t id);

void epsl_array_idx_fail();

void epsl_div_0_fail();

void epsl_array_empty_fail();

#endif
