#define __STDC_WANT_LIB_EXT2__ 1

#include <stdlib.h>
#include <stdio.h>
#include <inttypes.h>
#include <string.h>
#include <math.h>
#include <stdbool.h>

#ifndef __has_builtin
#define __has_builtin(x) 0
#endif

#if !__has_builtin(__builtin_expect)
#define __builtin_expect(expr, val) expr
#endif

#define ERR_START "FATAL ERROR: "

// elem types are 64 bit unsigned
// upper 62 bits are the size
// second to lowest bit is whether it's a pointer
// lowest bit is whether the value is nullable

struct Array {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    void *content;
};

extern inline uint64_t calc_new_capacity(uint64_t cap) {
    return 1+(cap*3)/2;
}

void increment_length(struct Array *array, uint64_t elem_size) {
    uint64_t length = array->length;
    uint64_t capacity = array->capacity;
    array->length = length+1;
    if (capacity == length) {
        // Current growth factor: 1.5
        uint64_t new_capacity = calc_new_capacity(capacity);
        array->capacity = new_capacity;
        array->content = realloc(array->content, elem_size*new_capacity);
    }
}

// Will grow capacity only to required amount
void require_capacity(struct Array *array, uint64_t required, uint64_t elem_size) {
    if (array->capacity < required) {
        array->capacity = required;
        array->content = realloc(array->content, elem_size*required);
    }
}

// Will grow capacity and then apply growth factor
void increace_capacity(struct Array *array, uint64_t required, uint64_t elem_size) {
    if (array->capacity < required) {
        uint64_t new_capacity = calc_new_capacity(required);
        array->capacity = new_capacity;
        array->content = realloc(array->content, elem_size*new_capacity);
    }
}

extern inline uint64_t min1(uint64_t val) {
    return val == 0 ? 1 : val;
}

void shrink_mem(struct Array *array, uint64_t elem_size) {
    uint64_t new_capacity = min1(array->length);
    array->content = realloc(array->content, elem_size*new_capacity);
    array->capacity = new_capacity;
}

void remove_at(struct Array *array, uint64_t idx, uint64_t elem_size) {
    uint64_t length = array->length--;
    char* content = array->content;
    char* deststart = content+idx*elem_size;
    char* srcstart = deststart+elem_size;
    memmove((void*)deststart, (void*)srcstart, elem_size*(length-idx-1));
}

const char *const INSERTSPACE_IDX_ERR = ERR_START "Cannot insert space outside bounds of array\n";

void insert_space(struct Array *array, uint64_t idx, uint64_t elem_size) {
    uint64_t length = array->length;
    if (__builtin_expect(idx > length, 0)) {
        fflush(stdout);
        fwrite(INSERTSPACE_IDX_ERR, strlen(INSERTSPACE_IDX_ERR), 1, stderr);
        exit(1);
    }
    increment_length(array, elem_size);
    char* content = array->content;
    char* srcstart = content+idx*elem_size;
    char* deststart = srcstart+elem_size;
    memmove((void*)deststart, (void*)srcstart, elem_size*(length-idx));
}

void increment_array_ref_counts(const struct Array *array, uint64_t elem) {
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

struct Array *clone_array(const struct Array *array, uint64_t elem) {
    struct Array *new_array = malloc(sizeof(struct Array));
    new_array->ref_counter = 0;
    uint64_t capacity = array->capacity;
    new_array->capacity = capacity;
    new_array->length = array->length;
    uint64_t elem_size = elem >> 2;
    uint64_t size = capacity * elem_size;
    void *content = malloc(size);
    memcpy(content, array->content, size);
    new_array->content = content;
    increment_array_ref_counts(array, elem);
    return new_array;
}

void extend_array(struct Array *array1, const struct Array *array2, uint64_t elem) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t new_len = len1 + len2;
    uint64_t elem_size = elem >> 2;
    increace_capacity(array1, new_len, elem_size);
    array1->length = new_len;
    increment_array_ref_counts(array2, elem);
    memcpy(array1->content+len1*elem_size, array2->content, len2*elem_size);
}

struct Array *concat_arrays(const struct Array *array1, const struct Array *array2, uint64_t elem) {
    struct Array *new_array = malloc(sizeof(struct Array));
    new_array->ref_counter = 0;
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t new_len = len1 + len2;
    new_array->length = new_len;
    uint64_t new_cap = calc_new_capacity(new_len);
    new_array->capacity = new_cap;
    uint64_t elem_size = elem >> 2;
    void *content = malloc(elem_size*new_cap);
    uint64_t size1 = len1*elem_size;
    uint64_t size2 = len2*elem_size;
    memcpy(content, array1->content, size1);
    memcpy(content+size1, array2->content, size2);
    new_array->content = content;
    increment_array_ref_counts(new_array, elem);
    return new_array;
}

struct Array *empty_array(uint64_t elem_size) {
    struct Array *array = malloc(sizeof(struct Array));
    array->ref_counter = 0;
    array->length = 0;
    array->capacity = 1;
    array->content = malloc(elem_size);
    return array;
}

struct Array *blank_array(uint64_t elem_size) {
    struct Array *array = malloc(sizeof(struct Array));
    array->ref_counter = 0;
    array->length = 0;
    array->capacity = 10;
    array->content = malloc(10*elem_size);
    return array;
}

void print(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    fflush(stdout);
}

void println(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    putc('\n', stdout);
}

extern inline char *formatW8() {
    static char *result = "%"PRIu8;
    return result;
}

extern inline char *formatW16() {
    static char *result = "%"PRIu16;
    return result;
}

extern inline char *formatW32() {
    static char *result = "%"PRIu32;
    return result;
}

extern inline char *formatW64() {
    static char *result = "%"PRIu64;
    return result;
}

extern inline char *formatZ8() {
    static char *result = "%"PRId8;
    return result;
}

extern inline char *formatZ16() {
    static char *result = "%"PRId16;
    return result;
}

extern inline char *formatZ32() {
    static char *result = "%"PRId32;
    return result;
}

extern inline char *formatZ64() {
    static char *result = "%"PRId64;
    return result;
}

const char *const SLICE_NEG_LEN_ERR = ERR_START "Slice end index must be after slice start index\n";
const char *const SLICE_INDEX_ERR = ERR_START "Slice end index out of range\n";

struct Array *slice_array(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem) {
    if (__builtin_expect(start > end, 0)) {
        fflush(stdout);
        fwrite(SLICE_NEG_LEN_ERR, strlen(SLICE_NEG_LEN_ERR), 1, stderr);
        exit(1);
    }

    if (__builtin_expect(end >= array->length, 0)) {
        fflush(stdout);
        fwrite(SLICE_INDEX_ERR, strlen(SLICE_INDEX_ERR), 1, stderr);
        exit(1);
    }

    struct Array *slice = malloc(sizeof(struct Array));
    slice->ref_counter = 0;
    uint64_t len = end - start + 1;
    slice->capacity = len;
    slice->length = len;
    uint64_t elem_size = elem >> 2;
    uint64_t size = elem_size * len;
    void *content = malloc(size);
    slice->content = content;
    memcpy(content, ((char*)array->content)+(start*elem_size), size);
    increment_array_ref_counts(slice, elem);
    return slice;
}

// Only works on strings
uint64_t count_chr(const struct Array *str, char chr) {
    uint64_t counter = 0;
    uint64_t len = str->length;
    char *content = str->content;
    for (uint64_t i = 0; i < len; i++) {
        if (content[i] == chr) counter++;
    }
    return counter;
}

struct Array *nest(const struct Array *arr, uint64_t elem) {
    char *arr_content = (char*)arr->content;
    uint64_t len = arr->length;
    struct Array *result = malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->capacity = len;
    result->length = len;
    struct Array **result_content = malloc(sizeof(struct Array*)*len);
    result->content = result_content;
    uint64_t elem_size = elem >> 2;
    for (uint64_t i = 0; i < len; i++) {
        struct Array *sub = malloc(sizeof(struct Array));
        sub->ref_counter = 1;
        sub->capacity = 1;
        sub->length = 1;
        void *value = arr_content+(i*elem_size);
        void *content = malloc(elem_size);
        memcpy(content, value, elem_size);
        sub->content = content;
        result_content[i] = sub;
        if (elem&1) continue;
        ++*(uint64_t*)value;
    }
    return result;
}

struct Array *join(const struct Array *arr, const struct Array *sep, uint64_t elem) {
    uint64_t elem_size = elem >> 2;
    struct Array *result = malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->length = 0;
    result->capacity = 1;
    result->content = malloc(elem_size);
    uint64_t arr_len = arr->length;
    struct Array **arr_content = arr->content;
    for (uint64_t i = 0; i < arr_len; i++) {
        if (i > 0) extend_array(result, sep, elem);
        extend_array(result, arr_content[i], elem);
    }
    return result;
}

const int32_t MAGIC_INVALID_PARSED_INT = -2147483647 + 69927;

int32_t parse_int(const struct Array *str) {
    int32_t result = 0;
    int32_t multiplier = 1;
    int32_t sign = 1;
    int valid = 0;
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
            valid = 1;
            continue;
        }
        if (chr == '_' || chr == ',') continue;
        return MAGIC_INVALID_PARSED_INT;
    }
    if (valid) return result * sign;
    return MAGIC_INVALID_PARSED_INT;
}

extern inline int32_t get_magic_invalid_parsed_int() {
    return MAGIC_INVALID_PARSED_INT;
}

double parse_float(const struct Array *str) {
    char *content = str->content;
    uint64_t length = str->length;
    int64_t dot = 0;
    for (; dot < length; dot++) {
        if (content[dot] == '.') goto has_dot;
    }

    // no dot
    int32_t ival = parse_int(str);
    if (ival == MAGIC_INVALID_PARSED_INT) return NAN;
    return ival;

has_dot:
    if (length == 1) return NAN;
    int valid = 0;
    double result = 0;
    double multiplier = 0.1;
    double sign = 1;
    for (int64_t i = 1; i + dot < length; i++) {
        char chr = content[i + dot];
        if ('0' <= chr && chr <= '9') {
            result += (chr - '0') * multiplier;
            multiplier *= 0.1;
            valid = 1;
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
            valid = 1;
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
    if (valid) return result * sign;
    return NAN;
}

extern inline bool is_NaN_32(float val) {
    return isnan(val) != 0;
}

extern inline bool is_NaN_64(double val) {
    return isnan(val) != 0;
}

struct Array *read_input_line() {
    // Maybe replace with a real readline solution like GNU readline?
    struct Array *result = blank_array(sizeof(char));
    while (1) {
        int chr = getchar();
        if (chr == EOF || chr == '\n' || chr == '\r') return result;
        uint64_t length = result->length;
        increment_length(result, sizeof(char));
        ((char*)result->content)[length] = (char)chr;
    }
}

struct File {
    uint64_t ref_counter;
    FILE *file;
    int32_t mode;
    int32_t open;
};

#define _FILE_READ_MODE 1
#define _FILE_WRITE_MODE 2
#define _FILE_APPEND_MODE 4
#define _FILE_BINARY_MODE 8

// returns File?
struct File *open_file(struct Array *string, int32_t mode) {
    char mode_str[4];
    int i = 0;

    if (mode&_FILE_WRITE_MODE) {
        if (mode&_FILE_APPEND_MODE)
            return NULL;
        mode_str[i++] = 'w';
        if (mode&_FILE_READ_MODE)
            mode_str[i++] = '+';
    } else if (mode&_FILE_READ_MODE) {
        mode_str[i++] = 'r';
        if (mode&_FILE_APPEND_MODE)
            mode_str[i++] = '+';
    } else if (mode&_FILE_APPEND_MODE) {
        mode_str[i++] = 'a';
    } else {
        return NULL;
    }

    if (mode&_FILE_BINARY_MODE) {
        mode_str[i++] = 'b';
    }

    mode_str[i] = '\0';

    uint64_t len = string->length;
    increment_length(string, 1);
    char* content = string->content;
    content[len] = '\0';
    string->length = len;

    FILE *C_file = fopen(content, mode_str);
    if (C_file == NULL) return NULL;

    struct File *file = malloc(sizeof(struct File));
    file->ref_counter = 0;
    file->file = C_file;
    file->mode = mode;
    file->open = 1;

    return file;
}

extern inline int32_t FILE_READ_MODE() {
    return _FILE_READ_MODE;
}

extern inline int32_t FILE_WRITE_MODE() {
    return _FILE_WRITE_MODE;
}

extern inline int32_t FILE_APPEND_MODE() {
    return _FILE_APPEND_MODE;
}

extern inline int32_t FILE_BINARY_MODE() {
    return _FILE_BINARY_MODE;
}

extern inline bool file_open(const struct File *file) {
    return file->open;
}

extern inline int32_t file_mode(const struct File *file) {
    return file->mode;
}

bool close_file(struct File *file) {
    if (file->open) {
        if (fclose(file->file) == 0) {
            file->open = 0;
            return true;
        }
        return false;
    } else {
        return false;
    }
}

int64_t file_length(const struct File *file) {
    if (file->open) {
        FILE *fp = file->file;
        long start_pos = ftell(fp);
        fseek(fp, 0, SEEK_END);
        uint64_t length = (uint64_t)ftell(fp);
        fseek(fp, start_pos, SEEK_SET);
        return length;
    } else {
        return -1;
    }
}

int64_t file_pos(const struct File *file) {
    if (file->open) {
        return (uint64_t)ftell(file->file);
    } else {
        return -1;
    }
}

// returns: Str?
struct Array *read_all_file(const struct File *file) {
    if (file->open) {
        uint64_t file_len = file_length(file);
        if (file_len == -1) return empty_array(sizeof(char));
        uint64_t cur_pos = file_pos(file);
        if (cur_pos == -1) return empty_array(sizeof(char));
        uint64_t remaining_text = (uint64_t)(file_len - cur_pos);
        uint64_t capacity = remaining_text;
        if (capacity == 0) capacity = 1;
        struct Array *result = malloc(sizeof(struct Array));
        result->ref_counter = 0;
        result->capacity = capacity;
        result->length = remaining_text;
        char *content = malloc(capacity);
        result->content = content;
        size_t read = fread(content, remaining_text, 1, file->file);
        if (read != 1) {
            free(result);
            free(content);
            return NULL;
        }
        return result;
    } else {
        return NULL;
    }
}

// returns: Str?
struct Array *read_some_file(const struct File *file, uint64_t amount) {
    if (file->open) {
        uint64_t capacity = amount;
        if (capacity == 0) capacity = 1;
        struct Array *result = malloc(sizeof(struct Array));
        result->ref_counter = 0;
        result->capacity = capacity;
        result->length = amount;
        char *content = malloc(capacity);
        result->content = content;
        size_t read = fread(content, amount, 1, file->file);
        if (read != 1) {
            free(result);
            free(content);
            return NULL;
        }
        return result;
    } else {
        return NULL;
    }
}

int32_t set_file_pos(const struct File *file, uint64_t pos) {
    if (file->open) {
        return fseek(file->file, (long)pos, SEEK_SET) == 0;
    }
    return false;
}

int32_t jump_file_pos(const struct File *file, uint64_t amount) {
    if (file->open) {
        return fseek(file->file, (long)amount, SEEK_CUR) == 0;
    }
    return false;
}

static bool read_line_EOF = false;

// returns: Str?
struct Array *read_file_line(const struct File *file) {
    read_line_EOF = false;
    if (file->open) {
        char *content = NULL;
        size_t capacity = 0;
        int64_t len = (int64_t)getline(&content, &capacity, file->file);
        if (len == -1) {
            free(content);
            read_line_EOF = true;
            return NULL;
        }
        if (content[len-1] == '\n') len--;
        struct Array *result = malloc(sizeof(struct Array));
        result->ref_counter = 0;
        result->capacity = capacity;
        result->content = content;
        result->length = len;
        return result;
    }
    return NULL;
}

extern inline bool read_line_reached_EOF() {
    return read_line_EOF;
}

// returns [Str]?
struct Array *read_file_lines(const struct File *file) {
    struct Array *result = blank_array(sizeof(struct Array));
    while (1) {
        struct Array *line = read_file_line(file);
        if (read_line_EOF) {
            return result;
        }
        if (line == NULL) {
            for (uint64_t i = 0; i < result->length; i++) {
                struct Array *str = ((struct Array**)result->content)[i];
                free(str->content);
                free(str);
            }
            free(result->content);
            free(result);
            return NULL;
        }
        uint64_t length = result->length;
        increment_length(result, sizeof(struct Array));
        ((struct Array**)result->content)[length] = line;
        line->ref_counter = 1;
    }
}

bool write_to_file(const struct File *file, const struct Array *text) {
    if (file->open) {
        uint64_t len = text->length;
        return fwrite(text->content, len, 1, file->file) == 1;
    } else {
        return false;
    }
}

void free_file(struct File *file) {
    // We don't need a check for if the file is already closed, becauuse
    // close_file contains one itself
    close_file(file);
    free(file);
}

void abort_(const struct Array *string) {
    fflush(stdout);
    fwrite(string->content, string->length, 1, stderr);
    putc('\n', stderr);
    exit(1);
}

struct Array *make_blank_array(uint64_t len, uint64_t elem_size) {
    struct Array *result = malloc(sizeof(struct Array));
    result->ref_counter = 0;
    uint64_t cap = min1(len);
    result->capacity = cap;
    result->length = len;
    void *content = calloc(cap, elem_size);
    result->content = content;
    return result;
}

extern inline void sort_array(struct Array *array, uint64_t elem_size, int (*compar)(const void*, const void*)) {
    qsort(array->content, array->length, elem_size, compar);
}

struct Array *repeat_array(const struct Array *array, uint64_t times, uint64_t elem) {
    struct Array *result = malloc(sizeof(struct Array));
    result->ref_counter = 0;
    uint64_t src_len = array->length;
    uint64_t new_len = src_len * times;
    result->capacity = new_len;
    result->length = new_len;
    uint64_t elem_size = elem >> 2;
    uint64_t src_size = src_len*elem_size;
    char *content = malloc(new_len*elem_size);
    for (uint64_t i = 0; i < times; i++) {
        memcpy(content+i*src_size, array->content, src_size);
    }
    result->content = content;
    increment_array_ref_counts(result, elem);
    return result;
}

const char *const NULL_FAIL_MESSAGE = ERR_START "Expected non-null value, found null\n";

void null_value_fail() {
    fflush(stdout);
    fwrite(NULL_FAIL_MESSAGE, strlen(NULL_FAIL_MESSAGE), 1, stderr);
    exit(1);
}

const char *const TOO_FEW_PLACEHOLDERS_MESSAGE = ERR_START "Not enough placeholders for given number of values\n";
const char *const TOO_MANY_PLACEHOLDERS_MESSAGE = ERR_START "Too many placeholders for given number of values\n";
const char *const FORMAT_STRING_PLACEHOLDER = "{}";

struct Array *format_string(struct Array *template_, struct Array *values[], uint32_t value_count) {
    size_t placeholder_len = strlen(FORMAT_STRING_PLACEHOLDER);
    uint32_t template_Len = template_->length;
    char *template_Content = template_->content;
    uint64_t result_len = template_Len - value_count * placeholder_len;
    for (uint32_t i = 0; i < value_count; i++) {
        result_len += values[i]->length;
    }

    uint64_t result_cap = result_len + 1;
    uint32_t value_idx = 0;
    char *result = malloc(result_cap);
    uint64_t result_idx = 0;
    uint64_t seg_start = 0;
    uint64_t template_iter_len = template_Len - placeholder_len + 1;
    for (uint64_t i = 0; i < template_iter_len; i++) {
        if (memcmp(template_Content + i, FORMAT_STRING_PLACEHOLDER, placeholder_len) == 0) {
            if (__builtin_expect(value_idx == value_count, 0)) {
                fflush(stdout);
                fwrite(TOO_MANY_PLACEHOLDERS_MESSAGE, strlen(TOO_MANY_PLACEHOLDERS_MESSAGE), 1, stderr);
                exit(1);
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
    }

    if (__builtin_expect(value_idx < value_count, 0)) {
        fflush(stdout);
        fwrite(TOO_FEW_PLACEHOLDERS_MESSAGE, strlen(TOO_FEW_PLACEHOLDERS_MESSAGE), 1, stderr);
        exit(1);
    }

    memcpy(result + result_idx, template_Content + seg_start, template_Len - seg_start);

    struct Array *result_arr = malloc(sizeof(struct Array));
    result_arr->ref_counter = 0;
    result_arr->capacity = result_cap;
    result_arr->length = result_len;
    result_arr->content = result;
    return result_arr;
}

const char *const ARR_INDEX_ERR = ERR_START "Specified array index is greater or equal to array length\n";

void array_idx_fail() {
    fflush(stdout);
    fwrite(ARR_INDEX_ERR, strlen(ARR_INDEX_ERR), 1, stderr);
    exit(1);
}

const char *const DIV_0_ERR = ERR_START "Cannot divide an integer by 0\n";

void div_0_fail() {
    fflush(stdout);
    fwrite(DIV_0_ERR, strlen(DIV_0_ERR), 1, stderr);
    exit(1);
}
