#include <stdlib.h>
#include <stdint.h>

void *cfile_getPtr() {
    uint32_t *data = malloc(sizeof(uint32_t));
    *data = 12;
    return data;
}

uint32_t cfile_usePtr(void *data) {
    uint32_t result = *(uint32_t*)data;
    free(data);
    return result;
}
