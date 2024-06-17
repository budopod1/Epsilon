#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>
#include <arpa/inet.h>

#define ERR_START "FATAL ERROR IN packing: "

struct ByteArray {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    char *content;
};

inline bool _packing_isNetworkByteOrder() {
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

const char *const BAD_LEN_FOR_DOUBLE_ERR = ERR_START "Array is not the correct length to be decoded into a double";

double packing_unpackDouble(struct ByteArray *arr) {
    if (arr->length != sizeof(double)) {
        fflush(stdout);
        fwrite(BAD_LEN_FOR_DOUBLE_ERR, strlen(BAD_LEN_FOR_DOUBLE_ERR), 1, stderr);
        exit(1);
    }
    double d;
    if (_packing_isNetworkByteOrder()) {
        memcpy(&d, arr->content, sizeof(double));
    } else {
        _packing_memcpyReversed(&d, arr->content, sizeof(double));
    }
    return d;
}
