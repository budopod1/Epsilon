#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>
#include <arpa/inet.h>

#ifndef __has_builtin
#define __has_builtin(x) 0
#endif

#if !__has_builtin(__builtin_expect)
#define __builtin_expect(expr, val) expr
#endif

#define __FLOAT16_EXISTS__ 0
#ifdef __is_identifier
#if __is_identifier(_Float16)
#define __FLOAT16_EXISTS__ 1
#endif
#endif

#define ERR_START "FATAL ERROR IN packing: "

struct ByteArray {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    char *content;
};

extern inline bool _packing_isNetworkByteOrder() {
    return htons(1) == 1;
}

void _packing_memcpyReversed(void *dest, void *const src, size_t amount) {
    char *dest_a = (char*)dest + amount;
    char *src_a = (char*)src;
    for (size_t i = 0; i < amount; i++) {
        *(--dest_a) = *(src_a++);
    }
}

struct ByteArray *_packing_packFloating(void *floating, size_t floating_size) {
    struct ByteArray *arr = malloc(sizeof(struct ByteArray));
    arr->refCounter = 0;
    arr->capacity = floating_size;
    arr->length = floating_size;
    arr->content = malloc(floating_size);
    if (_packing_isNetworkByteOrder()) {
        memcpy(arr->content, floating, floating_size);
    } else {
        _packing_memcpyReversed(arr->content, floating, floating_size);
    }
    return arr;
}

#if __FLOAT16_EXISTS__
struct ByteArray *packing_packHalf(_Float16 floating) {
    return _packing_packFloating(&floating, sizeof(floating));
}
#endif

struct ByteArray *packing_packFloat(float floating) {
    return _packing_packFloating(&floating, sizeof(floating));
}

struct ByteArray *packing_packDouble(double floating) {
    return _packing_packFloating(&floating, sizeof(floating));
}

#if __FLOAT128__
struct ByteArray *packing_packQuadruple(__float128 floating) {
    return _packing_packFloating(&floating, sizeof(floating));
}
#endif

const char *const BAD_LEN_FOR_FLOATING_ERR = ERR_START "Not enough remaining space in the array to read a %d bit floating point number\n";

void packing_unpackFloating(struct ByteArray *arr, uint64_t pos, void *floating, size_t floating_size) {
    if (__builtin_expect(pos + floating_size >= arr->length, 0)) {
        fflush(stdout);
        fprintf(stderr, BAD_LEN_FOR_FLOATING_ERR, 8 * (int)floating_size);
        exit(1);
    }

    char *data = arr->content + pos;
    if (_packing_isNetworkByteOrder()) {
        memcpy(floating, data, floating_size);
    } else {
        _packing_memcpyReversed(floating, data, floating_size);
    }
}

#if __FLOAT16_EXISTS__
_Float16 packing_unpackHalf(struct ByteArray *arr, uint64_t pos) {
    _Float16 result;
    packing_unpackFloating(arr, pos, &result, sizeof(result));
    return result;
}
#endif

float packing_unpackFloat(struct ByteArray *arr, uint64_t pos) {
    float result;
    packing_unpackFloating(arr, pos, &result, sizeof(result));
    return result;
}

double packing_unpackDouble(struct ByteArray *arr, uint64_t pos) {
    double result;
    packing_unpackFloating(arr, pos, &result, sizeof(result));
    return result;
}

#if __FLOAT128__
__float128 packing_unpackQuadruple(struct ByteArray *arr, uint64_t pos) {
    __float128 result;
    packing_unpackFloating(arr, pos, &result, sizeof(result));
    return result;
}
#endif
