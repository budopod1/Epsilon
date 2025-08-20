#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include "builtins.h"

#define __FLOAT16_EXISTS__ 0
#ifdef __is_identifier
#if __is_identifier(_Float16)
#define __FLOAT16_EXISTS__ 1
#endif
#endif

#define ERR_START "FATAL ERROR IN conversion: "

struct ByteArray {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    char *content;
};

static bool is_network_byte_order(void) {
    union {
        uint16_t num;
        char arr[2];
    } tester = {0x0102};
    return tester.arr[0] == 1;
}

static void memcpy_reversed(void *restrict dest, void *const restrict src, size_t amount) {
    char *dest_a = (char*)dest + amount;
    char *src_a = (char*)src;
    for (size_t i = 0; i < amount; i++) {
        *--dest_a = *(src_a++);
    }
}

static struct ByteArray *pack_floating(void *floating, size_t floating_size) {
    struct ByteArray *arr = epsl_malloc(sizeof(struct ByteArray));
    arr->ref_counter = 0;
    arr->capacity = floating_size;
    arr->length = floating_size;
    arr->content = epsl_malloc(floating_size);
    if (is_network_byte_order()) {
        memcpy(arr->content, floating, floating_size);
    } else {
        memcpy_reversed(arr->content, floating, floating_size);
    }
    return arr;
}

#if __FLOAT16_EXISTS__
struct ByteArray *conversion_pack_half(_Float16 floating) {
    return pack_floating(&floating, sizeof(floating));
}
#endif

struct ByteArray *conversion_pack_float(float floating) {
    return pack_floating(&floating, sizeof(floating));
}

struct ByteArray *conversion_pack_double(double floating) {
    return pack_floating(&floating, sizeof(floating));
}

#if __FLOAT128__
struct ByteArray *conversion_pack_quadruple(__float128 floating) {
    return pack_floating(&floating, sizeof(floating));
}
#endif

void conversion_unpack_floating(struct ByteArray *arr, uint64_t pos, void *floating, size_t floating_size) {
    if (__builtin_expect(pos + floating_size >= arr->length, 0)) {
        epsl_panicf(
            ERR_START "Not enough remaining space in the array to read a %d bit floating point number",
            8 * (int)floating_size
        );
    }

    char *data = arr->content + pos;
    if (is_network_byte_order()) {
        memcpy(floating, data, floating_size);
    } else {
        memcpy_reversed(floating, data, floating_size);
    }
}

#if __FLOAT16_EXISTS__
_Float16 conversion_unpack_half(struct ByteArray *arr, uint64_t pos) {
    _Float16 result;
    conversion_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}
#endif

float conversion_unpack_float(struct ByteArray *arr, uint64_t pos) {
    float result;
    conversion_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}

double conversion_unpack_double(struct ByteArray *arr, uint64_t pos) {
    double result;
    conversion_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}

#if __FLOAT128__
__float128 conversion_unpack_quadruple(struct ByteArray *arr, uint64_t pos) {
    __float128 result;
    conversion_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}
#endif

#define parse_str_to_int_inner(base, fail_val)\
    if (base < 2 || base > 26) {\
        epsl_panicf(\
            ERR_START "The base must be in the range 2 to 36, got %d", base\
        );\
    }\
    for (; i < arr->length; i++) {\
        result *= base;\
        char digit = arr->content[i];\
        if (digit >= '0' && digit <= ((base > 10) ? '9' : '0' + base - 1)) {\
            result += digit - '0';\
        } else if (base > 10 && ((digit >= 'a' && digit <= 'a' + base - 11)\
            || (digit >= 'A' && digit <= 'A' + base - 11))) {\
            result += (digit & 0x1f) + 9;\
        } else {\
            return fail_val;\
        }\
    }

#define parse_str_to_unsigned_fn(bits)\
    uint##bits##_t conversion_str_to_W##bits(struct ByteArray *arr) {\
        uint##bits##_t result = 0;\
        uint64_t i = 0;\
        parse_str_to_int_inner(10, UINT##bits##_MAX)\
        return result;\
    }

#define parse_str_to_signed_fn(bits)\
    int##bits##_t conversion_str_to_Z##bits(struct ByteArray *arr) {\
        int##bits##_t result = 0;\
        bool negative = arr->length > 0 && arr->content[0] == '-';\
        uint64_t i = !!negative;\
        parse_str_to_int_inner(10, INT##bits##_MIN)\
        return negative ? -result : result;\
    }

#define parse_based_str_to_unsigned_fn(bits)\
    uint##bits##_t conversion_based_str_to_W##bits(struct ByteArray *arr, uint32_t base) {\
        uint##bits##_t result = 0;\
        uint64_t i = 0;\
        parse_str_to_int_inner(base, UINT##bits##_MAX)\
        return result;\
    }

#define parse_based_str_to_signed_fn(bits)\
    int##bits##_t conversion_based_str_to_Z##bits(struct ByteArray *arr, uint32_t base) {\
        int##bits##_t result = 0;\
        bool negative = arr->length > 0 && arr->content[0] == '-';\
        uint64_t i = !!negative;\
        parse_str_to_int_inner(base, INT##bits##_MIN)\
        return negative ? -result : result;\
    }

parse_str_to_unsigned_fn(32)

parse_str_to_unsigned_fn(64)

parse_str_to_signed_fn(32)

parse_str_to_signed_fn(64)

parse_based_str_to_unsigned_fn(32)

parse_based_str_to_unsigned_fn(64)

parse_based_str_to_signed_fn(32)

parse_based_str_to_signed_fn(64)
