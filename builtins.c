#include <stdlib.h>
#include <stdio.h>
#include <inttypes.h>
#include <string.h>
#include <math.h>

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
        uint64_t newCapacity = 1+(capacity*3)/2;
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
    uint64_t newCapacity = array->length;
    if (newCapacity < 1) newCapacity = 1;
    array->content = realloc(array->content, elemSize*newCapacity);
    array->capacity = newCapacity;
}

void removeAt(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    array->length = length-1;
    char* content = array->content;
    char* deststart = content+idx*elemSize;
    char* srcstart = deststart+elemSize;
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx-1));
}

void insertSpace(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    incrementLength(array, elemSize);
    char* content = array->content;
    char* srcstart = content+idx*elemSize;
    char* deststart = srcstart+elemSize;
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx));
}

void incrementArrayRefCounts(const struct Array *array, uint64_t elem) {
    if (elem&1) return;
    uint64_t elemSize = elem >> 1;
    uint64_t length = array->length;
    char *content = array->content;
    for (uint64_t i = 0; i < length; i++) {
        (**(uint64_t**)(content+i*elemSize))++;
    }
}

void alwaysIncrementArrayRefCounts(const struct Array *array, uint64_t elemSize) {
    uint64_t length = array->length;
    char *content = array->content;
    for (uint64_t i = 0; i < length; i++) {
        (**(uint64_t**)(content+i*elemSize))++;
    }
}

struct Array *clone(const struct Array *array, uint64_t elem) {
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

void extend(struct Array *array1, const struct Array *array2, uint64_t elem) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t newLen = len1 + len2;
    uint64_t elemSize = elem >> 1;
    requireCapacity(array1, newLen, elemSize);
    array1->length = newLen;
    incrementArrayRefCounts(array2, elem);
    memcpy(array1->content+len1, array2->content, len2*elemSize);
}

struct Array *concat(const struct Array *array1, const struct Array *array2, uint64_t elem) {
    struct Array *newArray = malloc(sizeof(struct Array));
    newArray->refCounter = 0;
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    uint64_t newLen = len1 + len2;
    newArray->length = newLen;
    uint64_t newCap = newLen;
    if (newCap < 1) newCap = 1;
    newArray->capacity = newCap;
    uint64_t elemSize = elem >> 1;
    void *content = malloc(elemSize*newCap);
    memcpy(content, array1->content, len1*elemSize);
    memcpy(content+len1, array2->content, len2*elemSize);
    newArray->content = content;
    incrementArrayRefCounts(newArray, elem);
    return newArray;
}

struct Array *emptyArray(uint64_t elemSize) {
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = 0;
    array->capacity = 1;
    array->content = malloc(elemSize);
    return array;
}

struct Array *rangeArray1(int32_t end) {
    if (end <= 0) return emptyArray(sizeof(int32_t));
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = end;
    int32_t capacity = end;
    if (capacity < 1) capacity = 1;
    array->capacity = capacity;
    int32_t *content = malloc(sizeof(int32_t)*capacity);
    array->content = content;
    for (int32_t i = 0; i < end; i++) {
        content[i] = i;
    }
    return array;
}

struct Array *rangeArray2(int32_t start, int32_t end) {
    int32_t length = end - start;
    if (length <= 0) return emptyArray(sizeof(int32_t));
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = length;
    int32_t capacity = length;
    if (capacity < 1) capacity = 1;
    array->capacity = capacity;
    int32_t *content = malloc(sizeof(int32_t)*capacity);
    array->content = content;
    for (int32_t i = 0; i < length; i++) {
        content[i] = start + i;
    }
    return array;
}

struct Array *rangeArray3(int32_t start, int32_t end, int32_t step) {
    if (step == 0) return emptyArray(sizeof(int32_t));
    int32_t absstep = abs(step);
    int32_t dif = end - start;
    if (dif <= 0) return emptyArray(sizeof(int32_t));
    int32_t length = dif/absstep + (dif%absstep > 0);
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = length;
    int32_t capacity = length;
    if (capacity < 1) capacity = 1;
    array->capacity = capacity;
    int32_t *content = malloc(sizeof(int32_t)*capacity);
    array->content = content;
    for (int32_t i = 0; i < length; i++) {
        content[i] = (step>0?start:end) + step*i;
    }
    return array;
}

void print(struct Array *string) {
    // this assumes that the string is an array of chars
    uint64_t len = string->length;
    incrementLength(string, 1);
    char* content = string->content;
    content[len] = '\0';
    printf("%s", content);
    string->length = len;
}

void println(struct Array *string) {
    // this assumes that the string is an array of chars
    uint64_t len = string->length;
    incrementLength(string, 1);
    char* content = string->content;
    content[len] = '\0';
    puts(content);
    string->length = len;
}

char *formatW8() {
    static char *result = "%"PRIu8;
    return result;
}

char *formatW16() {
    static char *result = "%"PRIu16;
    return result;
}

char *formatW32() {
    static char *result = "%"PRIu32;
    return result;
}

char *formatW64() {
    static char *result = "%"PRIu64;
    return result;
}

char *formatZ8() {
    static char *result = "%"PRId8;
    return result;
}

char *formatZ16() {
    static char *result = "%"PRId16;
    return result;
}

char *formatZ32() {
    static char *result = "%"PRId32;
    return result;
}

char *formatZ64() {
    static char *result = "%"PRId64;
    return result;
}

// only works on strings
void leftPad(struct Array *str, uint64_t length, char chr) {
    uint64_t curLen = str->length;
    if (curLen < length) {
        requireCapacity(str, length, 1);
        char *content = str->content;
        str->length = length;
        for (uint64_t i = curLen; i <= length; i++) {
            content[i] = chr;
        }
    }
}

// only works on strings
void rightPad(struct Array *str, uint64_t length, char chr) {
    uint64_t curLen = str->length;
    if (curLen < length) {
        requireCapacity(str, length, 1);
        char *content = str->content;
        str->length = length;
        uint64_t reqLen = length - curLen;
        memmove(content+reqLen, content, curLen);
        for (uint64_t i = 0; i < reqLen; i++) {
            content[i] = chr;
        }
    }
}

struct Array *slice(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem) {
    struct Array *slice = malloc(sizeof(struct Array));
    slice->refCounter = 0;
    uint64_t len = end - start + 1;
    slice->capacity = len;
    slice->length = len;
    uint64_t elemSize = elem >> 1;
    uint64_t size = elemSize * len;
    void *content = malloc(size);
    slice->content = content;
    memcpy(content, ((char*)array->content)+(start*elemSize), size);
    incrementArrayRefCounts(slice, elem);
    return slice;
}

int arrayEqual(const struct Array *array1, const struct Array *array2, uint64_t elemSize) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    if (len1 == len2) {
        return memcmp(array1->content, array2->content, len1*elemSize) == 0;
    } else {
        return 0;
    }
}

// Only works on strings
uint64_t countChr(const struct Array *str, char chr) {
    uint64_t counter = 0;
    uint64_t len = str->length;
    char *content = str->content;
    for (uint64_t i = 0; i < len; i++) {
        if (content[i] == chr) counter++;
    }
    return counter;
}

uint64_t count(const struct Array *arr, const struct Array *seg, uint64_t elemSize) {
    uint64_t counter = 0;
    uint64_t arrLen = arr->length;
    uint64_t segLen = seg->length;
    char *segContent = seg->content;
    char *content = arr->content;
    uint64_t segSize = segLen * elemSize;
    for (uint64_t i = 0; segLen + i <= arrLen;) {
        if (memcmp(content+(elemSize*i), segContent, segSize) == 0) {
            counter++;
            i += segLen;
        } else {
            i++;
        }
    }
    return counter;
}

uint64_t overlapCount(const struct Array *arr, const struct Array *seg, uint64_t elemSize) {
    uint64_t counter = 0;
    uint64_t arrLen = arr->length;
    uint64_t segLen = seg->length;
    char *segContent = seg->content;
    char *content = arr->content;
    uint64_t segSize = segLen * elemSize;
    for (uint64_t i = 0; segLen + i <= arrLen; i++) {
        if (memcmp(content+(elemSize*i), segContent, segSize) == 0) {
            counter++;
        }
    }
    return counter;
}

struct Array *nest(const struct Array *arr, uint64_t elem) {
    char *arrContent = (char*)arr->content;
    uint64_t len = arr->length;
    struct Array *result = malloc(sizeof(struct Array));
    result->refCounter = 0;
    result->capacity = len;
    result->length = len;
    struct Array **resultContent = malloc(sizeof(struct Array*)*len);
    result->content = resultContent;
    uint64_t elemSize = elem >> 1;
    for (uint64_t i = 0; i < len; i++) {
        struct Array *sub = malloc(sizeof(struct Array));
        sub->refCounter = 1;
        sub->capacity = 1;
        sub->length = 1;
        void *value = arrContent+(i*elemSize);
        void *content = malloc(elemSize);
        memcpy(content, value, elemSize);
        sub->content = content;
        resultContent[i] = sub;
        if (elem&1) continue;
        uint64_t valueRefCounter = *((uint64_t*)value);
        valueRefCounter++;
    }
    return result;
}

struct Array *split(const struct Array *arr, const struct Array *seg, uint64_t elem) {
    uint64_t segLen = seg->length;
    if (segLen == 0) return nest(arr, elem);
    struct Array *result = malloc(sizeof(struct Array));
    size_t ptrSize = sizeof(struct Array*);
    result->refCounter = 0;
    result->capacity = 10;
    result->length = 0;
    uint64_t elemSize = elem >> 1;
    result->content = malloc(ptrSize*10);
    uint64_t arrLen = arr->length;
    uint64_t sectionCount = 0;
    uint64_t partStart = 0;
    char *segContent = seg->content;
    char *content = arr->content;
    uint64_t segSize = segLen * elemSize;
    for (uint64_t i = 0; segLen + i <= arrLen;) {
        char *iPtr = content+(elemSize*i);
        if (memcmp(iPtr, segContent, segSize) == 0) {
            uint64_t sectionLen = i - partStart;
            struct Array *section = malloc(sizeof(struct Array));
            section->refCounter = 1;
            uint64_t capacity = sectionLen;
            if (capacity == 0) capacity = 1;
            section->capacity = capacity;
            section->length = sectionLen;
            void *secContent = malloc(capacity*elemSize);
            memcpy(
                secContent, content+(elemSize*partStart), 
                sectionLen*elemSize
            );
            section->content = secContent;
            incrementArrayRefCounts(section, elem);
            incrementLength(result, ptrSize);
            ((struct Array**)result->content)[sectionCount++] = section;
            i += segLen;
            partStart = i;
        } else {
            i++;
        }
    }
    {
        uint64_t sectionLen = arrLen - partStart;
        struct Array *section = malloc(sizeof(struct Array));
        section->refCounter = 1;
        uint64_t capacity = sectionLen;
        if (capacity == 0) capacity = 1;
        section->capacity = capacity;
        section->length = sectionLen;
        void *secContent = malloc(capacity*elemSize);
        memcpy(
            secContent, content+(elemSize*partStart), 
            sectionLen*elemSize
        );
        section->content = secContent;
        incrementArrayRefCounts(section, elem);
        incrementLength(result, ptrSize);
        ((struct Array**)result->content)[sectionCount] = section;
    }
    return result;
}

int startsWith(const struct Array *arr, const struct Array *sub, uint64_t elemSize) {
    uint64_t subLen = sub->length;
    if (subLen > arr->length) return 0;
    return memcmp(arr->content, sub->content, subLen*elemSize) == 0;
}

int endsWith(const struct Array *arr, const struct Array *sub, uint64_t elemSize) {
    uint64_t subLen = sub->length;
    uint64_t arrLen = arr->length;
    if (subLen > arrLen) return 0;
    uint64_t startIdx = arrLen - subLen;
    char *startPtr = ((char*)arr->content)+startIdx*elemSize;
    return memcmp(startPtr, sub->content, subLen*elemSize) == 0;
}

struct Array *join(const struct Array *arr, const struct Array *sep, uint64_t elem) {
    uint64_t elemSize = elem << 1;
    struct Array *result = malloc(sizeof(struct Array));
    result->refCounter = 0;
    result->length = 0;
    result->capacity = 1;
    result->content = malloc(elemSize);
    uint64_t arrLen = arr->length;
    struct Array **arrContent = arr->content;
    for (uint64_t i = 0; i < arrLen; i++) {
        if (i > 0) extend(result, sep, elem);
        extend(result, arrContent[i], elem);
    }
    return result;
}

int64_t indexOfSubsection(const struct Array *arr, const struct Array *sub, uint64_t elemSize) {
    uint64_t arrLen = arr->length;
    uint64_t subLen = sub->length;
    if (subLen > arrLen) return -1;
    if (arrLen == 0) return -1;
    if (subLen == 0) return 0;
    char *arrContent = arr->content;
    char *subContent = sub->content;
    for (uint64_t i = 0; i <= arrLen-subLen; i++) {
        if (memcmp(subContent, arrContent+(i*elemSize), subLen * elemSize) == 0) {
            return i;
        }
    }
    return -1;
}

int32_t parseInt(const struct Array *str) {
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
        return 2147483647;
    }
    if (valid) return (result * sign);
    return 2147483647;
}

int32_t isValidParsedInt(int32_t i) {
    return i != 2147483647;
}

float parseFloat(const struct Array *str) {
    char *content = str->content;
    uint64_t length = str->length;
    int64_t dot = 0;
    int noDot = 1;
    for (; dot < length; dot++) {
        if (content[dot] == '.') {
            noDot = 0;
            break;
        }
    }
    if (noDot) {
        int32_t ival = parseInt(str);
        if (isValidParsedInt(ival)) return ival;
        return NAN;
    }
    if (length == 1) return NAN;
    int valid = 0;
    float result = 0;
    float multiplier = 0.1;
    float sign = 1;
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
    if (valid) return (result * sign);
    return NAN;
}

int32_t isValidParsedFloat(float f) {
    return !isnan(f);
}

struct Array *readInputLine() {
    // Maybe replace with a real readline solution like GNU readline?
    struct Array *result = emptyArray(sizeof(char));
    while (1) {
        int chr = getchar();
        if (chr == EOF || chr == '\n' || chr == '\r') return result;
        uint64_t length = result->length;
        incrementLength(result, sizeof(char));
        ((char*)result->content)[length] = (char)chr;
    }
}
