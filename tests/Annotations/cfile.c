#include <stdint.h>

extern int32_t frwd_test(int32_t arg);

int32_t cfile_callTest(int32_t num) {
    return frwd_test(2 * num);
}
