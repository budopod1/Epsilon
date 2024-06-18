#define __STDC_WANT_LIB_EXT2__ 1

#include <stdlib.h>
#include <stdio.h>
#include <inttypes.h>
#include <string.h>
#include <math.h>
#include <stdbool.h>

#define ERR_START "FATAL ERROR: "

// elem types are 64 bit unsigned
// upper 62 bits are the size
// second to lowest bit is whether it's a pointer
// lowest bit is whether the value is nullable

struct Array {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    void *content;
};

inline uint64_t calcNewCapacity(uint64_t cap) {
    return 1+(cap*3)/2;
}

void incrementLength(struct Array *array, uint64_t elemSize) {
    uint64_t length = array->length;
    uint64_t capacity = array->capacity;
    array->length = length+1;
    if (capacity == length) {
        // Current growth factor: 1.5
        uint64_t newCapacity = calcNewCapacity(capacity);
        array->capacity = newCapacity;
        array->content = realloc(array->content, elemSize*newCapacity);
    }
}

// Will grow capacity only to required amount
void requireCapacity(struct Array *array, uint64_t required, uint64_t elemSize) {
    if (array->capacity < required) {
        array->capacity = required;
        array->content = realloc(array->content, elemSize*required);
    }
}

// Will grow capacity and then apply growth factor
void increaceCapacity(struct Array *array, uint64_t required, uint64_t elemSize) {
    if (array->capacity < required) {
        uint newCapacity = calcNewCapacity(required);
        array->capacity = newCapacity;
        array->content = realloc(array->content, elemSize*newCapacity);
    }
}

void shrinkMem(struct Array *array, uint64_t elemSize) {
    uint64_t newCapacity = array->length;
    if (newCapacity < 1) newCapacity = 1;
    array->content = realloc(array->content, elemSize*newCapacity);
    array->capacity = newCapacity;
}

const char *const REMOVEAT_IDX_ERR = ERR_START "Cannot remove item at index outside bounds of array\n";

void removeAt(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    if (idx >= length) {
        fflush(stdout);
        fwrite(REMOVEAT_IDX_ERR, strlen(REMOVEAT_IDX_ERR), 1, stderr);
        exit(1);
    }
    array->length = length-1;
    char* content = array->content;
    char* deststart = content+idx*elemSize;
    char* srcstart = deststart+elemSize;
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx-1));
}

const char *const INSERTSPACE_IDX_ERR = ERR_START "Cannot insert space outside bounds of array\n";

void insertSpace(struct Array *array, uint64_t idx, uint64_t elemSize) {
    uint64_t length = array->length;
    if (idx > length) {
        fflush(stdout);
        fwrite(INSERTSPACE_IDX_ERR, strlen(INSERTSPACE_IDX_ERR), 1, stderr);
        exit(1);
    }
    incrementLength(array, elemSize);
    char* content = array->content;
    char* srcstart = content+idx*elemSize;
    char* deststart = srcstart+elemSize;
    memmove((void*)deststart, (void*)srcstart, elemSize*(length-idx));
}

void incrementArrayRefCounts(const struct Array *array, uint64_t elem) {
    if (elem&2) return;
    uint64_t elemSize = elem >> 2;
    uint64_t length = array->length;
    char *content = array->content;
    for (uint64_t i = 0; i < length; i++) {
        uint64_t *item = *(uint64_t**)(content+i*elemSize);
        if (elem&1) {
            if (item == NULL) continue;
        }
        (*item)++;
    }
}

struct Array *clone(const struct Array *array, uint64_t elem) {
    struct Array *newArray = malloc(sizeof(struct Array));
    newArray->refCounter = 0;
    uint64_t capacity = array->capacity;
    newArray->capacity = capacity;
    newArray->length = array->length;
    uint64_t elemSize = elem >> 2;
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
    uint64_t elemSize = elem >> 2;
    increaceCapacity(array1, newLen, elemSize);
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
    uint64_t newCap = calcNewCapacity(newLen);
    newArray->capacity = newCap;
    uint64_t elemSize = elem >> 2;
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

struct Array *blankArray(uint64_t elemSize) {
    struct Array *array = malloc(sizeof(struct Array));
    array->refCounter = 0;
    array->length = 0;
    array->capacity = 10;
    array->content = malloc(10*elemSize);
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

void print(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    fflush(stdout);
}

void println(const struct Array *string) {
    fwrite(string->content, string->length, 1, stdout);
    putc('\n', stdout);
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
        memset(content+curLen, chr, length-curLen);
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
        memset(content, chr, reqLen);
    }
}
const char *const SLICE_NEG_LEN_ERR = ERR_START "Slice end index must be after slice start index\n";
const char *const SLICE_INDEX_ERR = ERR_START "Slice end index out of range\n";

struct Array *slice(const struct Array *array, uint64_t start, uint64_t end, uint64_t elem) {
    if (start > end) {
        fflush(stdout);
        fwrite(SLICE_NEG_LEN_ERR, strlen(SLICE_NEG_LEN_ERR), 1, stderr);
        exit(1);
    }
    if (end >= array->length) {
        fflush(stdout);
        fwrite(SLICE_INDEX_ERR, strlen(SLICE_INDEX_ERR), 1, stderr);
        exit(1);
    }
    struct Array *slice = malloc(sizeof(struct Array));
    slice->refCounter = 0;
    uint64_t len = end - start + 1;
    slice->capacity = len;
    slice->length = len;
    uint64_t elemSize = elem >> 2;
    uint64_t size = elemSize * len;
    void *content = malloc(size);
    slice->content = content;
    memcpy(content, ((char*)array->content)+(start*elemSize), size);
    incrementArrayRefCounts(slice, elem);
    return slice;
}

bool arrayEqual(const struct Array *array1, const struct Array *array2, uint64_t elemSize) {
    uint64_t len1 = array1->length;
    uint64_t len2 = array2->length;
    if (len1 == len2) {
        return memcmp(array1->content, array2->content, len1*elemSize) == 0;
    } else {
        return false;
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
    uint64_t elemSize = elem >> 2;
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
    uint64_t ptrSize = sizeof(struct Array*);
    struct Array *result = blankArray(ptrSize);
    uint64_t elemSize = elem >> 2;
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

bool startsWith(const struct Array *arr, const struct Array *sub, uint64_t elemSize) {
    uint64_t subLen = sub->length;
    if (subLen > arr->length) return 0;
    return memcmp(arr->content, sub->content, subLen*elemSize) == 0;
}

bool endsWith(const struct Array *arr, const struct Array *sub, uint64_t elemSize) {
    uint64_t subLen = sub->length;
    uint64_t arrLen = arr->length;
    if (subLen > arrLen) return 0;
    uint64_t startIdx = arrLen - subLen;
    char *startPtr = ((char*)arr->content)+startIdx*elemSize;
    return memcmp(startPtr, sub->content, subLen*elemSize) == 0;
}

struct Array *join(const struct Array *arr, const struct Array *sep, uint64_t elem) {
    uint64_t elemSize = elem >> 2;
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

const int32_t MAGIC_INVALID_PARSED_INT = -2147483647 + 69927;

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
        return MAGIC_INVALID_PARSED_INT;
    }
    if (valid) return result * sign;
    return MAGIC_INVALID_PARSED_INT;
}

int32_t getMagicInvalidParsedInt() {
    return MAGIC_INVALID_PARSED_INT;
}

double parseFloat(const struct Array *str) {
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
        if (ival == MAGIC_INVALID_PARSED_INT) return NAN;
        return ival;
    }
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

bool isNaN32(float val) {
    return isnan(val) != 0;
}

bool isNaN64(double val) {
    return isnan(val) != 0;
}

struct Array *readInputLine() {
    // Maybe replace with a real readline solution like GNU readline?
    struct Array *result = blankArray(sizeof(char));
    while (1) {
        int chr = getchar();
        if (chr == EOF || chr == '\n' || chr == '\r') return result;
        uint64_t length = result->length;
        incrementLength(result, sizeof(char));
        ((char*)result->content)[length] = (char)chr;
    }
}

struct File {
    uint64_t refCounter;
    FILE *file;
    int32_t mode;
    int32_t open;
};

int32_t _FILE_READ_MODE = 1;
int32_t _FILE_WRITE_MODE = 2;
int32_t _FILE_APPEND_MODE = 4;
int32_t _FILE_BINARY_MODE = 8;

// returns File?
struct File *openFile(struct Array *string, int32_t mode) {
    char modeStr[4];
    int i = 0;
    
    if (mode&_FILE_WRITE_MODE) {
        if (mode&_FILE_APPEND_MODE)
            return NULL;
        modeStr[i++] = 'w';
        if (mode&_FILE_READ_MODE)
            modeStr[i++] = '+';
    } else if (mode&_FILE_READ_MODE) {
        modeStr[i++] = 'r';
        if (mode&_FILE_APPEND_MODE)
            modeStr[i++] = '+';
    } else if (mode&_FILE_APPEND_MODE) {
        modeStr[i++] = 'a';
    } else {
        return NULL;
    }
    
    if (mode&_FILE_BINARY_MODE) {
        modeStr[i++] = 'b';
    }
    
    modeStr[i] = '\0';

    uint64_t len = string->length;
    incrementLength(string, 1);
    char* content = string->content;
    content[len] = '\0';
    string->length = len;
    
    FILE *cFile = fopen(content, modeStr);
    if (cFile == NULL) return NULL;

    struct File *file = malloc(sizeof(struct File));
    file->refCounter = 0;
    file->file = cFile;
    file->mode = mode;
    file->open = 1;

    return file;
}

int32_t FILE_READ_MODE() {
    return _FILE_READ_MODE;
}

int32_t FILE_WRITE_MODE() {
    return _FILE_WRITE_MODE;
}

int32_t FILE_APPEND_MODE() {
    return _FILE_APPEND_MODE;
}

int32_t FILE_BINARY_MODE() {
    return _FILE_BINARY_MODE;
}

bool fileOpen(const struct File *file) {
    return file->open;
}

int32_t fileMode(const struct File *file) {
    return file->mode;
}

bool closeFile(struct File *file) {
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

int64_t fileLength(const struct File *file) {
    if (file->open) {
        FILE *fp = file->file;
        long startPos = ftell(fp);
        fseek(fp, 0, SEEK_END); 
        uint64_t length = (uint64_t)ftell(fp);
        fseek(fp, startPos, SEEK_SET);
        return length;
    } else {
        return -1;
    }
}

int64_t filePos(const struct File *file) {
    if (file->open) {
        return (uint64_t)ftell(file->file);
    } else {
        return -1;
    }
}

// returns: Str?
struct Array *readAllFile(const struct File *file) {
    if (file->open) {
        uint64_t fileLen = fileLength(file);
        if (fileLen == -1) return emptyArray(sizeof(char));
        uint64_t curPos = filePos(file);
        if (curPos == -1) return emptyArray(sizeof(char));
        uint64_t remainingText = (uint64_t)(fileLen - curPos);
        uint64_t capacity = remainingText;
        if (capacity == 0) capacity = 1;
        struct Array *result = malloc(sizeof(struct Array));
        result->refCounter = 0;
        result->capacity = capacity;
        result->length = remainingText;
        char *content = malloc(capacity);
        result->content = content;
        size_t read = fread(content, remainingText, 1, file->file);
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
struct Array *readSomeFile(const struct File *file, uint64_t amount) {
    if (file->open) {
        uint64_t capacity = amount;
        if (capacity == 0) capacity = 1;
        struct Array *result = malloc(sizeof(struct Array));
        result->refCounter = 0;
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

int32_t setFilePos(const struct File *file, uint64_t pos) {
    if (file->open) {
        return fseek(file->file, (long)pos, SEEK_SET) == 0;
    }
    return false;
}

int32_t jumpFilePos(const struct File *file, uint64_t amount) {
    if (file->open) {
        return fseek(file->file, (long)amount, SEEK_CUR) == 0;
    }
    return false;
}

bool readLineEOF = false;

// returns: Str?
struct Array *readFileLine(const struct File *file) {
    readLineEOF = false;
    if (file->open) {
        char *content = NULL;
        size_t capacity = 0;
        int64_t len = (int64_t)getline(&content, &capacity, file->file);
        if (len == -1) {
            free(content);
            readLineEOF = true;
            return NULL;
        }
        if (content[len-1] == '\n') len--;
        struct Array *result = malloc(sizeof(struct Array));
        result->refCounter = 0;
        result->capacity = capacity;
        result->content = content;
        result->length = len;
        return result;
    }
    return NULL;
}

bool readLineReachedEOF() {
    return readLineEOF;
}

// returns [Str]?
struct Array *readFileLines(const struct File *file) {
    struct Array *result = blankArray(sizeof(struct Array));
    while (1) {
        struct Array *line = readFileLine(file);
        if (readLineEOF) {
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
        incrementLength(result, sizeof(struct Array));
        ((struct Array**)result->content)[length] = line;
        line->refCounter = 1;
    }
}

bool writeToFile(const struct File *file, const struct Array *text) {
    if (file->open) {
        uint64_t len = text->length;
        return fwrite(text->content, len, 1, file->file) == 1;
    } else {
        return false;
    }
}

void freeFile(struct File *file) {
    // We don't need a check for if the file is already closed, becauuse
    // closeFile contains one itself
    closeFile(file);
    free(file);
}

void abort_(const struct Array *string) {
    fflush(stdout);
    fwrite(string->content, string->length, 1, stderr);
    putc('\n', stderr);
    exit(1);
}

struct Array *makeBlankArray(uint64_t size, uint64_t elemSize) {
    struct Array *result = malloc(sizeof(struct Array));
    result->refCounter = 0;
    result->capacity = size;
    result->length = size;
    uint64_t byteCount = size*elemSize;
    void *content = calloc(1, byteCount);
    result->content = content;
    return result;
}

void sortArray(struct Array *array, uint64_t elemSize, int (*compar)(const void*, const void*)) {
    qsort(array->content, array->length, elemSize, compar);
}

struct Array *repeatArray(const struct Array *array, uint64_t times, uint64_t elem) {
    struct Array *result = malloc(sizeof(struct Array));
    result->refCounter = 0;
    uint64_t srcLen = array->length;
    uint64_t newLen = srcLen * times;
    result->capacity = newLen;
    result->length = newLen;
    uint64_t elemSize = elem >> 2;
    uint64_t srcSize = srcLen*elemSize;
    char *content = malloc(newLen*elemSize);
    for (uint64_t i = 0; i < times; i++) {
        memcpy(content+i*srcSize, array->content, srcSize);
    }
    result->content = content;
    incrementArrayRefCounts(result, elem);
    return result;
}

const char *const NULL_FAIL_MESSAGE = ERR_START "Expected non-null value, found null\n";

void verifyNotNull(const char *val) {
    if (val == NULL) {
        fflush(stdout);
        fwrite(NULL_FAIL_MESSAGE, strlen(NULL_FAIL_MESSAGE), 1, stderr);
        exit(1);
    }
}

const char *const TOO_FEW_PLACEHOLDERS_MESSAGE = ERR_START "Not enough placeholders for given number of values";
const char *const TOO_MANY_PLACEHOLDERS_MESSAGE = ERR_START "Too many placeholders for given number of values";
const char *const FORMAT_STRING_PLACEHOLDER = "{}";

struct Array *formatString(struct Array *template_, struct Array *values[], uint32_t valueCount) {
    size_t placeholder_len = strlen(FORMAT_STRING_PLACEHOLDER);
    uint32_t template_Len = template_->length;
    char *template_Content = template_->content;
    uint64_t resultLen = template_Len - valueCount * placeholder_len;
    for (uint32_t i = 0; i < valueCount; i++) {
        resultLen += values[i]->length;
    }

    uint64_t resultCap = resultLen + 1;
    uint32_t valueIdx = 0;
    char *result = malloc(resultCap);
    uint64_t resultIdx = 0;
    uint64_t segStart = 0;
    for (uint64_t i = 0; i < template_Len; i++) {
        if (memcmp(template_Content + i, FORMAT_STRING_PLACEHOLDER, placeholder_len) == 0) {
            if (valueIdx == valueCount) {
                fflush(stdout);
                fwrite(TOO_FEW_PLACEHOLDERS_MESSAGE, strlen(TOO_FEW_PLACEHOLDERS_MESSAGE), 1, stderr);
                exit(1);
            }
            
            uint64_t segLen = i - segStart;
            memcpy(result + resultIdx, template_Content + segStart, segLen);
            resultIdx += segLen;
            
            struct Array *value = values[valueIdx++];
            uint64_t valueLen = value->length;
            memcpy(result + resultIdx, value->content, valueLen);
            resultIdx += valueLen;

            segStart = i + placeholder_len;
            i = segStart - 1;
        }
    }
    
    if (valueIdx < valueCount) {
        fflush(stdout);
        fwrite(TOO_MANY_PLACEHOLDERS_MESSAGE, strlen(TOO_MANY_PLACEHOLDERS_MESSAGE), 1, stderr);
        exit(1);
    }

    memcpy(result + resultIdx, template_Content + segStart, template_Len - segStart);
    
    struct Array *resultArr = malloc(sizeof(struct Array));
    resultArr->refCounter = 0;
    resultArr->capacity = resultCap;
    resultArr->length = resultLen;
    resultArr->content = result;
    return resultArr;
}
