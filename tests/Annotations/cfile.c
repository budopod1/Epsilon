#include <stdint.h>

union ___EPSL_PUBLIC_STOP;
extern int32_t frwd_test(int32_t arg);
union ___EPSL_PUBLIC_START;

int32_t call_test(int32_t num) {
    return frwd_test(2 * num);
}
