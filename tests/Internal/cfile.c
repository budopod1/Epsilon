#include <stdlib.h>
#include <stdint.h>

#define EPSL_COMMON_PREFIX "C_"

#ifdef EPSL_PROJECT
void *C_get_ptr() {
    uint32_t *data = malloc(sizeof(uint32_t));
    *data = 12;
    return data;
}
#endif

uint32_t C_use_ptr(void *data) {
    uint32_t result = *(uint32_t*)data;
    free(data);
    return result;
}
