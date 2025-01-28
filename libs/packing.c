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

#define ERR_START "FATAL ERROR IN packing: "

struct ByteArray {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    char *content;
};

static bool is_network_byte_order() {
    union {
        short num;
        char arr[2];
    } tester = {0x0102};
    return tester.arr[0] == 1;
}

static void memcpy_reversed(void *dest, void *const src, size_t amount) {
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
struct ByteArray *packing_pack_half(_Float16 floating) {
    return pack_floating(&floating, sizeof(floating));
}
#endif

struct ByteArray *packing_pack_float(float floating) {
    return pack_floating(&floating, sizeof(floating));
}

struct ByteArray *packing_pack_double(double floating) {
    return pack_floating(&floating, sizeof(floating));
}

#if __FLOAT128__
struct ByteArray *packing_pack_quadruple(__float128 floating) {
    return pack_floating(&floating, sizeof(floating));
}
#endif

void packing_unpack_floating(struct ByteArray *arr, uint64_t pos, void *floating, size_t floating_size) {
    if (__builtin_expect(pos + floating_size >= arr->length, 0)) {
        epsl_formatted_panic(
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
_Float16 packing_unpack_half(struct ByteArray *arr, uint64_t pos) {
    _Float16 result;
    packing_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}
#endif

float packing_unpack_float(struct ByteArray *arr, uint64_t pos) {
    float result;
    packing_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}

double packing_unpack_double(struct ByteArray *arr, uint64_t pos) {
    double result;
    packing_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}

#if __FLOAT128__
__float128 packing_unpack_quadruple(struct ByteArray *arr, uint64_t pos) {
    __float128 result;
    packing_unpack_floating(arr, pos, &result, sizeof(result));
    return result;
}
#endif
