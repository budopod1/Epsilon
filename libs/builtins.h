#ifndef BUILTINS_H

#define BUILTINS_H

#include <stdint.h>

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

extern inline uint64_t calc_new_capacity(uint64_t cap);

void increment_length(struct Array *array, uint64_t elem_size);

void require_capacity(struct Array *array, uint64_t required, uint64_t elem_size);

void increace_capacity(struct Array *array, uint64_t required, uint64_t elem_size);

void shrink_mem(struct Array *array, uint64_t elem_size);

void remove_at(struct Array *array, uint64_t idx, uint64_t elem_size);

void insert_space(struct Array *array, uint64_t idx, uint64_t elem_size);

void increment_array_ref_counts(const struct Array *array, uint64_t elem);

struct Array *clone_array(const struct Array *array, uint64_t elem);

void extend_array(struct Array *array1, const struct Array *array2, uint64_t elem);

struct Array *concat_arrays(const struct Array *array1, const struct Array *array2, uint64_t elem);

struct Array *blank_array(uint64_t elem_size);

void print(const struct Array *string);

void println(const struct Array *string);

extern inline char *formatW8();

extern inline char *formatW16();

extern inline char *formatW32();

extern inline char *formatW64();

extern inline char *formatZ8();

extern inline char *formatZ16();

extern inline char *formatZ32();

extern inline char *formatZ64();

struct Array *slice_array(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem);

struct Array *nest_array(const struct Array *arr, uint64_t elem);

struct Array *join_array(const struct Array *arr, const struct Array *sep, uint64_t elem);

int32_t parse_int(const struct Array *str);

int32_t get_magic_invalid_parsed_int();

double parse_float(const struct Array *str);

struct Array *read_input_line();

void abort_(const struct Array *string);

struct Array *make_blank_array(uint64_t len, uint64_t elem_size);

extern inline void sort_array(struct Array *array, uint64_t elem_size, int (*compar)(const void*, const void*));

struct Array *repeat_array(const struct Array *array, uint64_t times, uint64_t elem);

void null_value_fail();

struct Array *format_string(struct Array *template_, struct Array *values[], uint32_t value_count);

void array_idx_fail();

void div_0_fail();

void array_empty_fail();

#endif
