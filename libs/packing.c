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

struct ByteArray *packing_packDouble(double d) {
    struct ByteArray *arr = malloc(sizeof(struct ByteArray));
    arr->refCounter = 0;
    arr->capacity = sizeof(double);
    arr->length = sizeof(double);
    arr->content = malloc(sizeof(double));
    if (_packing_isNetworkByteOrder()) {
        memcpy(arr->content, &d, sizeof(double));
    } else {
        _packing_memcpyReversed(arr->content, &d, sizeof(double));
    }
    return arr;
}

const char *const BAD_LEN_FOR_DOUBLE_ERR = ERR_START "Array is not long enough, given the starting point, to be converted to a double\n";

double packing_unpackDouble(struct ByteArray *arr, uint64_t pos) {
    if (__builtin_expect(pos + sizeof(double) >= arr->length, 0)) {
        fflush(stdout);
        fwrite(BAD_LEN_FOR_DOUBLE_ERR, strlen(BAD_LEN_FOR_DOUBLE_ERR), 1, stderr);
        exit(1);
    }
    char *data = arr->content + pos;
    double d;
    if (_packing_isNetworkByteOrder()) {
        memcpy(&d, data, sizeof(double));
    } else {
        _packing_memcpyReversed(&d, data, sizeof(double));
    }
    return d;
}
