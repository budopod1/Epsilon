#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#include <stdlib.h>
#include <math.h>
#include <inttypes.h>
#include <stdbool.h>

#include "builtins.h"

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#ifndef M_E
#define M_E 2.71828182845904523536
#endif

struct C {
    uint64_t ref_counter;
    double real;
    double imag;
};

struct UIntArray {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    uint32_t *content;
};

struct IntArray {
    uint64_t ref_counter;
    uint64_t capacity;
    uint64_t length;
    int32_t *content;
};

double math_var_base_log(double argument, double base) {
    return log(argument) / log(base);
}

struct C *math_add_C(struct C *a, struct C *b) {
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    result->real = a->real + b->real;
    result->imag = a->imag + b->imag;
    return result;
}

struct C *math_sub_C(struct C *a, struct C *b) {
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    result->real = a->real - b->real;
    result->imag = a->imag - b->imag;
    return result;
}

struct C *math_mul_C(struct C *a, struct C *b) {
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    result->real = a->real * b->real - a->imag * b->imag;
    result->imag = a->real * b->imag + a->imag * b->real;
    return result;
}

struct C *math_div_C(struct C *a, struct C *b) {
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    double v = b->real * b->real + b->imag * b->imag;
    result->real = (a->real * b->real + a->imag * b->imag) / v;
    result->imag = (a->imag * b->real - a->real * b->imag) / v;
    return result;
}

struct C *math_exp_C(struct C *val) {
    double m = exp(val->real);
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    result->real = cos(val->imag) * m;
    result->imag = sin(val->imag) * m;
    return result;
}

struct C *math_exponent_to_cmplx(double base, double exponent) {
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    if (base >= 0) {
        result->real = pow(base, exponent);
        result->imag = 0;
        return result;
    } else {
        double m = pow(-base, exponent);
        result->real = cos(exponent * M_PI) * m;
        result->imag = sin(exponent * M_PI) * m;
        return result;
    }
}

struct C *math_cmplx_to_exponent(struct C *base, double exponent) {
    double theta = atan2(base->imag, base->real);
    double r = sqrt(base->real*base->real + base->imag*base->imag);
    double m = pow(r, exponent);
    struct C *result = epsl_malloc(sizeof(struct C));
    result->ref_counter = 0;
    result->real = cos(theta * exponent) * m;
    result->imag = sin(theta * exponent) * m;
    return result;
}

uint32_t math_usum(struct UIntArray *nums) {
    uint32_t result = 0;
    for (uint64_t i = 0; i < nums->length; i++) {
        result += nums->content[i];
    }
    return result;
}

uint32_t math_uprod(struct UIntArray *nums) {
    uint32_t result = 1;
    for (uint64_t i = 0; i < nums->length; i++) {
        result *= nums->content[i];
    }
    return result;
}

int32_t math_sum(struct IntArray *nums) {
    int32_t result = 0;
    for (uint64_t i = 0; i < nums->length; i++) {
        result += nums->content[i];
    }
    return result;
}

int32_t math_prod(struct IntArray *nums) {
    int32_t result = 1;
    for (uint64_t i = 0; i < nums->length; i++) {
        result *= nums->content[i];
    }
    return result;
}

uint32_t math_GCD(uint32_t n, uint32_t m) {
    if (m == 0) {
        return n;
    } else {
        return math_GCD(m, n % m);
    }
}

uint32_t math_array_GCD(struct UIntArray *nums) {
    uint32_t len = nums->length;
    if (len == 0) return 0;
    uint32_t gcd = nums->content[0];
    for (uint64_t i = 1; i < len; i++) {
        gcd = math_GCD(gcd, nums->content[i]);
    }
    return gcd;
}

uint32_t math_LCM(uint32_t n, uint32_t m) {
    uint32_t GCD = math_GCD(n, m);
    if (GCD == 0) return 0;
    return n * m / GCD;
}

uint32_t math_array_LCM(struct UIntArray *nums) {
    uint32_t GCD = math_array_GCD(nums);
    if (GCD == 0) return 0;
    return math_uprod(nums) / GCD;
}

double math_get_e(void) {
    return M_E;
}

bool math_is_finite_32(float val) {
    return isfinite(val) != 0;
}

bool math_is_infinite_32(float val) {
    return isinf(val) != 0;
}

bool math_is_finite_64(double val) {
    return isfinite(val) != 0;
}

bool math_is_infinite_64(double val) {
    return isinf(val) != 0;
}

int32_t math_fsign(double val) {
    if (val == 0) {
        return 0;
    } else if (val < 0) {
        return -1;
    } else {
        return 1;
    }
}

int32_t math_isign(int32_t val) {
    if (val == 0) {
        return 0;
    } else if (val < 0) {
        return -1;
    } else {
        return 1;
    }
}

int32_t math_icopysign(int32_t magnitude, int32_t direction) {
    return abs(magnitude) * math_isign(direction);
}

// sin, cos, tan, asin, acos, atan, atan2, sinh, cosh, tanh, asinh, acosh, atanh, sign, copysign are all llvm intrinsics or c builtins
