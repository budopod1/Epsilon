#include <stdlib.h>
#include <stdint.h>

void *get_ptr() {
    uint32_t *data = malloc(sizeof(uint32_t));
    *data = 12;
    return data;
}

uint32_t use_ptr(void *data) {
    uint32_t result = *(uint32_t*)data;
    free(data);
    return result;
}
