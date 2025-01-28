#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#include <stdlib.h>
#include <stdio.h>
#include <inttypes.h>
#include <string.h>
#include <math.h>
#include <stdbool.h>
#include <threads.h>
#include <stdarg.h>

#include "builtins.h"

void epsl_panic(const char *message, uint64_t message_len) {
    fflush(stdout);
    const char **error_stack_frame = epsl_error_stack;
    if (epsl_error_stack_top == error_stack_frame) {
        fputs("No traceback avaliable\n", stderr);
    } else {
        // 'last most call recent' has no meaning
        // I just couldn't figure out what Python's 'most recent call last'
        // meant until I asked ChatGPT
        fputs("Traceback (last most call recent):\n", stderr);
        do {
            fputs(*++error_stack_frame, stderr);
            fputc('\n', stderr);
        } while (error_stack_frame != epsl_error_stack_top);
    }
    fwrite(message, message_len, 1, stderr);
    fputc('\n', stderr);
    exit(1);
}

void epsl_formatted_panic(const char *format, ...) {
    va_list vargs;
    va_start(vargs, format);
    size_t msg_len = vsnprintf(NULL, 0, format, vargs);
    char buffer[msg_len+1];
    vsprintf(buffer, format, vargs);
    epsl_panic(buffer, msg_len);
    va_end(vargs);
}

#define ERR_START "FATAL ERROR: "

#define BUILTINS_PANIC(message) static const char *const panic_message = ERR_START message; EPSL_PANIC(panic_message)

void *epsl_memmove(void *dest, const void *src, uint64_t count) {
    return memmove(dest, src, count);
}

void *epsl_memset(void *dest, unsigned char ch, uint64_t count) {
    return memset(dest, ch, count);
}

int32_t epsl_memcmp(const void *lhs, const void *rhs, uint64_t count) {
    return memcmp(lhs, rhs, count);
}

void *epsl_memcpy(void *dest, const void *src, uint64_t count) {
    return memcpy(dest, src, count);
}

int32_t epsl_printf(const char *format, ...) {
    va_list vargs;
    va_start(vargs, format);
    int32_t result = vprintf(format, vargs);
    va_end(vargs);
    return result;
}

int32_t epsl_sprintf(char *buffer, const char *format, ...) {
    va_list vargs;
    va_start(vargs, format);
    int32_t result = vsprintf(buffer, format, vargs);
    va_end(vargs);
    return result;
}

int32_t epsl_snprintf(char *buffer, uint64_t bufsz, const char *format, ...) {
    va_list vargs;
    va_start(vargs, format);
    int32_t result = vsnprintf(buffer, bufsz, format, vargs);
    va_end(vargs);
    return result;
}

void epsl_exit(uint32_t status) {
    exit(status);
}

void epsl_out_of_memory_fail() {
    BUILTINS_PANIC("Out of memory");
}

void *epsl_malloc(uint64_t size) {
    void *result = malloc(size);
    if (__builtin_expect(result == NULL, 0)) {
        epsl_out_of_memory_fail();
    }
    return result;
}

void *epsl_calloc(uint64_t num, uint64_t size) {
    void *result = calloc(num, size);
    if (__builtin_expect(result == NULL, 0)) {
        epsl_out_of_memory_fail();
    }
    return result;
}

void *epsl_realloc(void *ptr, uint64_t new_size) {
    void *result = realloc(ptr, new_size);
    if (__builtin_expect(result == NULL, 0)) {
        epsl_out_of_memory_fail();
    }
    return result;
}

void epsl_nonresizable_array_fail() {
    BUILTINS_PANIC("The specified array can't be resized");
}

static inline uint64_t epsl_calc_new_capacity(uint64_t cap) {
    return 1+(cap*3)/2;
}

void epsl_increment_length(struct Array *array, uint64_t elem_size) {
    uint64_t length = array->length;
    uint64_t capacity = array->capacity;
    if (capacity == 0) epsl_nonresizable_array_fail();
    array->length = length+1;
    if (capacity == length) {
        // Current growth factor: 1.5
        uint64_t new_capacity = epsl_calc_new_capacity(capacity);
        array->capacity = new_capacity;
        array->content = epsl_realloc(array->content, elem_size*new_capacity);
    }
}

// Will grow capacity only to required amount
void epsl_require_capacity(struct Array *array, uint64_t required, uint64_t elem_size) {
    uint64_t capacity = array->capacity;
    if (capacity == 0) epsl_nonresizable_array_fail();
    if (capacity < required) {
        array->capacity = required;
        array->content = epsl_realloc(array->content, elem_size*required);
    }
}

// Will grow capacity and then apply growth factor
void epsl_increace_capacity(struct Array *array, uint64_t required, uint64_t elem_size) {
    uint64_t capacity = array->capacity;
    if (capacity == 0) epsl_nonresizable_array_fail();
    if (capacity < required) {
        uint64_t new_capacity = epsl_calc_new_capacity(required);
        array->capacity = new_capacity;
        array->content = epsl_realloc(array->content, elem_size*new_capacity);
    }
}

static inline uint64_t min1(uint64_t val) {
    return val == 0 ? 1 : val;
}

void epsl_shrink_mem(struct Array *array, uint64_t elem_size) {
    if (array->capacity == 0) epsl_nonresizable_array_fail();
    uint64_t new_capacity = min1(array->length);
    array->content = epsl_realloc(array->content, elem_size*new_capacity);
    array->capacity = new_capacity;
}

void epsl_remove_at(struct Array *array, uint64_t idx, uint64_t elem_size) {
    if (array->capacity == 0) epsl_nonresizable_array_fail();
    uint64_t length = array->length--;
    char* content = array->content;
    char* deststart = content+idx*elem_size;
    char* srcstart = deststart+elem_size;
    memmove((void*)deststart, (void*)srcstart, elem_size*(length-idx-1));
}

void epsl_insert_space(struct Array *array, uint64_t idx, uint64_t elem_size) {
    uint64_t length = array->length;
    if (__builtin_expect(idx > length, 0)) {
        BUILTINS_PANIC("Cannot insert space outside bounds of array");
    }
    epsl_increment_length(array, elem_size);
    char* content = array->content;
    char* srcstart = content+idx*elem_size;
    char* deststart = srcstart+elem_size;
    memmove((void*)deststart, (void*)srcstart, elem_size*(length-idx));
}

void epsl_increment_array_ref_counts(const struct Array *array, uint64_t elem) {
    if (elem & 2) return;
    uint64_t elem_size = elem >> 2;
    uint64_t length = array->length;
    char *content = array->content;
    if (elem & 1) {
        for (uint64_t i = 0; i < length; i++) {
            uint64_t *item = *(uint64_t**)(content+i*elem_size);
            if (item == NULL) continue;
            ++*item;
        }
    } else {
        for (uint64_t i = 0; i < length; i++) {
            uint64_t *item = *(uint64_t**)(content+i*elem_size);
            ++*item;
        }
    }
}

struct Array *epsl_clone_array(const struct Array *array, uint64_t elem) {
    struct Array *new_array = epsl_malloc(sizeof(struct Array));
    new_array->ref_counter = 0;
    uint64_t length = array->length;
    uint64_t capacity = array->capacity;
    capacity = capacity == 0 ? min1(length) : capacity;
    new_array->capacity = capacity;
    new_array->length = length;
    uint64_t elem_size = elem >> 2;
    uint64_t size = capacity * elem_size;
    void *content = epsl_malloc(size);
    memcpy(content, array->content, size);
    new_array->content = content;
    epsl_increment_array_ref_counts(array, elem);
    return new_array;
}

void epsl_extend_array(struct Array *array1, const struct Array *array2, uint64_t elem) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t new_len = len1 + len2;
    uint64_t elem_size = elem >> 2;
    epsl_increace_capacity(array1, new_len, elem_size);
    array1->length = new_len;
    epsl_increment_array_ref_counts(array2, elem);
    memcpy(array1->content+len1*elem_size, array2->content, len2*elem_size);
}

struct Array *epsl_concat_arrays(const struct Array *array1, const struct Array *array2, uint64_t elem) {
    struct Array *new_array = epsl_malloc(sizeof(struct Array));
    new_array->ref_counter = 0;
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t new_len = len1 + len2;
    new_array->length = new_len;
    uint64_t new_cap = epsl_calc_new_capacity(new_len);
    new_array->capacity = new_cap;
    uint64_t elem_size = elem >> 2;
    void *content = epsl_malloc(elem_size*new_cap);
    uint64_t size1 = len1*elem_size;
    uint64_t size2 = len2*elem_size;
    memcpy(content, array1->content, size1);
    memcpy(content+size1, array2->content, size2);
    new_array->content = content;
    epsl_increment_array_ref_counts(new_array, elem);
    return new_array;
}

struct Array *epsl_blank_array(uint64_t elem_size) {
    struct Array *array = epsl_malloc(sizeof(struct Array));
    array->ref_counter = 0;
    array->length = 0;
    array->capacity = 5;
    array->content = epsl_malloc(5*elem_size);
    return array;
}

void epsl_print(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    fflush(stdout);
}

void epsl_println(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    putc('\n', stdout);
}

extern inline char *epsl_format_W8() {
    static char *result = "%"PRIu8;
    return result;
}

extern inline char *epsl_format_W16() {
    static char *result = "%"PRIu16;
    return result;
}

extern inline char *epsl_format_W32() {
    static char *result = "%"PRIu32;
    return result;
}

extern inline char *epsl_format_W64() {
    static char *result = "%"PRIu64;
    return result;
}

extern inline char *epsl_format_Z8() {
    static char *result = "%"PRId8;
    return result;
}

extern inline char *epsl_format_Z16() {
    static char *result = "%"PRId16;
    return result;
}

extern inline char *epsl_format_Z32() {
    static char *result = "%"PRId32;
    return result;
}

extern inline char *epsl_format_Z64() {
    static char *result = "%"PRId64;
    return result;
}

struct Array *epsl_slice_array(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem) {
    if (__builtin_expect(start > end, 0)) {
        BUILTINS_PANIC("Slice start index can't be after slice end index");
    }

    if (__builtin_expect(end > array->length, 0)) {
        BUILTINS_PANIC("Slice end index out of range");
    }

    struct Array *slice = epsl_malloc(sizeof(struct Array));
    slice->ref_counter = 0;
    uint64_t len = end - start;
    slice->capacity = len;
    slice->length = len;
    uint64_t elem_size = elem >> 2;
    uint64_t size = elem_size * len;
    void *content = epsl_malloc(size);
    slice->content = content;
    memcpy(content, ((char*)array->content)+(start*elem_size), size);
    epsl_increment_array_ref_counts(slice, elem);
    return slice;
}

struct Array *epsl_nest_array(const struct Array *arr, uint64_t elem) {
    char *arr_content = (char*)arr->content;
    uint64_t len = arr->length;
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->capacity = len;
    result->length = len;
    struct Array **result_content = epsl_malloc(sizeof(struct Array*)*len);
    result->content = result_content;
    uint64_t elem_size = elem >> 2;
    for (uint64_t i = 0; i < len; i++) {
        struct Array *sub = epsl_malloc(sizeof(struct Array));
        sub->ref_counter = 1;
        sub->capacity = 1;
        sub->length = 1;
        void *value = arr_content+(i*elem_size);
        void *content = epsl_malloc(elem_size);
        memcpy(content, value, elem_size);
        sub->content = content;
        result_content[i] = sub;
        if (elem&1) continue;
        ++*(uint64_t*)value;
    }
    return result;
}

struct Array *epsl_join_array(const struct Array *arr, const struct Array *sep, uint64_t elem) {
    uint64_t elem_size = elem >> 2;
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->length = 0;
    result->capacity = 1;
    result->content = epsl_malloc(elem_size);
    uint64_t arr_len = arr->length;
    struct Array **arr_content = arr->content;
    for (uint64_t i = 0; i < arr_len; i++) {
        if (i > 0) epsl_extend_array(result, sep, elem);
        epsl_extend_array(result, arr_content[i], elem);
    }
    return result;
}

const int32_t MAGIC_INVALID_PARSED_INT = -2147483647;

int32_t epsl_parse_int(const struct Array *str) {
    int32_t result = 0;
    int32_t multiplier = 1;
    int32_t sign = 1;
    bool valid = false;
    char *content = str->content;
    uint64_t length = str->length;
    for (int64_t i = length-1; i >= 0; i--) {
        char chr = content[i];
        if (i == 0 && chr == '-') {
            sign = -1;
            continue;
        }
        if (i == 0 && chr == '+') continue;
        if ('0' <= chr && chr <= '9') {
            result += (chr - '0') * multiplier;
            multiplier *= 10;
            valid = true;
            continue;
        }
        if (chr == '_' || chr == ',') continue;
        return MAGIC_INVALID_PARSED_INT;
    }
    if (valid) return result * sign;
    return MAGIC_INVALID_PARSED_INT;
}

int32_t epsl_magic_invalid_parsed_int() {
    return MAGIC_INVALID_PARSED_INT;
}

double epsl_parse_float(const struct Array *str) {
    char *content = str->content;
    uint64_t length = str->length;
    int64_t dot = 0;
    for (; dot < length; dot++) {
        if (content[dot] == '.') goto has_dot;
    }

    // no dot
    int32_t ival = epsl_parse_int(str);
    if (ival == MAGIC_INVALID_PARSED_INT) return NAN;
    return ival;

has_dot:
    if (length == 1) return NAN;

    bool valid = false;
    double result = 0;
    double multiplier = 0.1;
    double sign = 1;

    for (int64_t i = 1; i + dot < length; i++) {
        char chr = content[i + dot];
        if ('0' <= chr && chr <= '9') {
            result += (chr - '0') * multiplier;
            multiplier *= 0.1;
            valid = true;
            continue;
        }
        if (chr == '_' || chr == ',') continue;
        return NAN;
    }

    multiplier = 1;

    for (int64_t i = 1; dot - i >= 0; i++) {
        char chr = content[dot - i];
        if ('0' <= chr && chr <= '9') {
            result += (chr - '0') * multiplier;
            multiplier *= 10;
            valid = true;
            continue;
        }
        if (chr == '_' || chr == ',') continue;
        if (dot - i == 0) {
            if (chr == '-') {
                sign = -1;
                continue;
            }
            if (chr == '+') continue;
        }
        return NAN;
    }

    if (valid) {
        return result * sign;
    } else {
        return NAN;
    }
}

struct Array *epsl_read_input_line() {
    // Maybe replace with a real readline solution like GNU readline?
    struct Array *result = epsl_blank_array(sizeof(char));
    while (true) {
        int chr = getchar();
        if (chr == EOF || chr == '\n' || chr == '\r') return result;
        uint64_t length = result->length;
        epsl_increment_length(result, sizeof(char));
        ((char*)result->content)[length] = (char)chr;
    }
}

void epsl_abort(const struct Array *string) {
    epsl_panic(string->content, string->length);
}

void epsl_abort_void() {
    EPSL_PANIC("abort");
}

struct Array *epsl_make_blank_array(uint64_t len, uint64_t elem_size) {
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    uint64_t cap = min1(len);
    result->capacity = cap;
    result->length = len;
    void *content = epsl_calloc(cap, elem_size);
    result->content = content;
    return result;
}

static thread_local int32_t (*thread_compar_base)(const void*, const void*);

static int compar_wrapper(const void *a, const void *b) {
    return (*thread_compar_base)(a, b);
}

void epsl_sort_array(struct Array *array, uint64_t elem_size, int32_t (*compar)(const void*, const void*)) {
    if (sizeof(int) == sizeof(int32_t)) {
        qsort(array->content, array->length, elem_size, compar);
    } else {
        thread_compar_base = compar;
        qsort(array->content, array->length, elem_size, &compar_wrapper);
    }
}

struct Array *epsl_repeat_array(const struct Array *array, uint64_t times, uint64_t elem) {
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    uint64_t src_len = array->length;
    uint64_t new_len = src_len * times;
    result->capacity = new_len;
    result->length = new_len;
    uint64_t elem_size = elem >> 2;
    uint64_t src_size = src_len*elem_size;
    char *content = epsl_malloc(new_len*elem_size);
    for (uint64_t i = 0; i < times; i++) {
        memcpy(content+i*src_size, array->content, src_size);
    }
    result->content = content;
    epsl_increment_array_ref_counts(result, elem);
    return result;
}

void epsl_null_value_fail() {
    BUILTINS_PANIC("Expected non-null value, found null");
}

static const char *const FORMAT_STRING_PLACEHOLDER = "{}";

struct Array *epsl_format_string(struct Array *template_, struct Array *values[], uint32_t value_count) {
    size_t placeholder_len = strlen(FORMAT_STRING_PLACEHOLDER);
    uint32_t template_Len = template_->length;
    char *template_Content = template_->content;
    uint64_t result_len = template_Len - value_count * placeholder_len;
    for (uint32_t i = 0; i < value_count; i++) {
        result_len += values[i]->length;
    }

    uint64_t result_cap = result_len + 1;
    uint32_t value_idx = 0;
    char *result = epsl_malloc(result_cap);
    uint64_t result_idx = 0;
    uint64_t seg_start = 0;
    uint64_t template_iter_len = template_Len - placeholder_len + 1;
    for (uint64_t i = 0; i < template_iter_len; i++) {
        if (memcmp(template_Content + i, FORMAT_STRING_PLACEHOLDER, placeholder_len))
            continue;

        if (__builtin_expect(value_idx == value_count, 0)) {
            BUILTINS_PANIC("Too many placeholders for given number of values");
        }

        uint64_t seg_len = i - seg_start;
        memcpy(result + result_idx, template_Content + seg_start, seg_len);
        result_idx += seg_len;

        struct Array *value = values[value_idx++];
        uint64_t value_len = value->length;
        memcpy(result + result_idx, value->content, value_len);
        result_idx += value_len;

        seg_start = i + placeholder_len;
        i = seg_start - 1;
    }

    if (__builtin_expect(value_idx < value_count, 0)) {
        BUILTINS_PANIC("Not enough placeholders for given number of values");
    }

    memcpy(result + result_idx, template_Content + seg_start, template_Len - seg_start);

    struct Array *result_arr = epsl_malloc(sizeof(struct Array));
    result_arr->ref_counter = 0;
    result_arr->capacity = result_cap;
    result_arr->length = result_len;
    result_arr->content = result;
    return result_arr;
}

void epsl_array_idx_fail() {
    BUILTINS_PANIC("Specified array index is greater or equal to array length");
}

void epsl_div_0_fail() {
    BUILTINS_PANIC("Cannot divide an integer by 0");
}

void epsl_array_empty_fail() {
    BUILTINS_PANIC("Expected an array with a nonzero length");
}
