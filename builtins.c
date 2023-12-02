#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>

// elem types are 64 bit unsigned
// upper 63 bits are the size
// lower 1 bit is whether it's a pointer

struct Array {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    void *content;
};

void incrementLength(struct Array *array, uint64_t elemSize) {
    uint64_t length = array->length;
    uint64_t capacity = array->capacity;
    array->length = length+1;
    if (capacity == length) {
        // Current growth factor: 1.5
        uint64_t newCapacity = (capacity*3)/2;
        array->capacity = newCapacity;
        array->content = realloc(array->content, elemSize*newCapacity);
    }
}

void requireCapacity(struct Array *array, uint64_t capacity, uint64_t elemSize) {
    if (array->capacity < capacity) {
        array->capacity = capacity;
        array->content = realloc(array->content, elemSize*capacity);
    }
}

void shrinkMem(struct Array *array, uint64_t elemSize) {
    uint64_t length = array->length;
    array->content = realloc(array->content, elemSize*length);
    array->capacity = length;
}

void removeAt(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    array->length = length-1;
    char* content = array->content;
    char* deststart = content+idx*elemSize;
    char* srcstart = deststart+elemSize;
    // TODO: test
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx-1));
}

void insertSpace(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    incrementLength(array, elemSize);
    char* content = array->content;
    char* srcstart = content+idx*elemSize;
    char* deststart = srcstart+elemSize;
    // TODO: test
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx));
}

void incrementArrayRefCounts(struct Array *array, uint64_t elem) {
    if (elem&1) return;
    uint64_t elemSize = elem >> 1;
    uint64_t length = array->length;
    char *content = array->content;
    for (uint64_t i = 0; i < length; i++) {
         // TODO: test
        (*(uint64_t*)(content+i*elem))++;
    }
}

void alwaysIncrementArrayRefCounts(struct Array *array, uint64_t elemSize) {
    uint64_t length = array->length;
    char *content = array->content;
    for (uint64_t i = 0; i < length; i++) {
         // TODO: test
        (*(uint64_t*)(content+i*elemSize))++;
    }
}

struct Array *clone(struct Array *array, uint64_t elem) {
    struct Array *newArray = malloc(sizeof(struct Array));
    newArray->refCounter = 0;
    uint64_t capacity = array->capacity;
    newArray->capacity = capacity;
    newArray->length = array->length;
    uint64_t elemSize = elem >> 1;
    uint64_t size = capacity * elemSize;
    void *content = malloc(size);
    memcpy(content, array->content, size);
    newArray->content = content;
    incrementArrayRefCounts(array, elem);
    return newArray;
}

void extend(struct Array *array1, struct Array *array2, uint64_t elem) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t newLen = len1 + len2;
    uint64_t elemSize = elem >> 1;
    requireCapacity(array1, newLen, elemSize);
    array1->length = newLen;
    incrementArrayRefCounts(array2, elem);
    memcpy(array1->content+len1, array2->content, len2*elemSize);
}

struct Array *join(struct Array *array1, struct Array *array2, uint64_t elem) {
    struct Array *newArray = malloc(sizeof(struct Array));
    newArray->refCounter = 0;
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t newLen = len1 + len2;
    newArray->length = newLen;
    newArray->capacity = newLen;
    uint64_t elemSize = elem >> 1;
    void *content = malloc(elemSize*newLen);
    memcpy(content, array1->content, len1*elemSize);
    memcpy(content+len1, array2->content, len2*elemSize);
    newArray->content = content;
    incrementArrayRefCounts(newArray, elem);
    return newArray;
}

struct Array *rangeArray1(int32_t end) {
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = end;
    array->capacity = end;
    int32_t *content = malloc(sizeof(int32_t)*end);
    array->content = content;
    for (int32_t i = 0; i < end; i++) {
        content[i] = i;
    }
    return array;
}

struct Array *rangeArray2(int32_t start, int32_t end) {
    int32_t length = end - start;
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = length;
    array->capacity = length;
    int32_t *content = malloc(sizeof(int32_t)*length);
    array->content = content;
    for (int32_t i = 0; i < length; i++) {
        content[i] = start + i;
    }
    return array;
}

struct Array *rangeArray3(int32_t start, int32_t end, int32_t step) {
    int32_t dif = end - start;
    int32_t length = dif/step + (dif%step > 0);
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = length;
    array->capacity = length;
    int32_t *content = malloc(sizeof(int32_t)*length);
    array->content = content;
    if (step > 0) {
        for (int32_t i = start; i < end; i += step) {
            content[i] = i;
        }
    } else {
        // TODO: implement
    }
    return array;
}
