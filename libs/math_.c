#include <stdlib.h>
#include <math.h>
#include <inttypes.h>
#include <stdbool.h>

struct C {
    uint64_t refCounter;
    double real;
    double imag;
};

struct UIntArray {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    uint32_t *content;
};

struct IntArray {
    uint64_t refCounter;
    uint64_t capacity;
    uint64_t length;
    int32_t *content;
};

double math_varBaseLog(double argument, double base) {
    return log(argument) / log(base);
}

struct C *math_addC(struct C *a, struct C *b) {
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
    result->real = a->real + b->real;
    result->imag = a->imag + b->imag;
    return result;
}

struct C *math_subC(struct C *a, struct C *b) {
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
    result->real = a->real - b->real;
    result->imag = a->imag - b->imag;
    return result;
}

struct C *math_mulC(struct C *a, struct C *b) {
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
    result->real = a->real * b->real - a->imag * b->imag;
    result->imag = a->real * b->imag + a->imag * b->real;
    return result;
}

struct C *math_divC(struct C *a, struct C *b) {
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
    double v = b->real * b->real + b->imag * b->imag;
    result->real = (a->real * b->real + a->imag * b->imag) / v;
    result->imag = (a->imag * b->real - a->real * b->imag) / v;
    return result;
}

struct C *math_expC(struct C *val) {
    double m = exp(val->real);
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
    result->real = cos(val->imag) * m;
    result->imag = sin(val->imag) * m;
    return result;
}

struct C *math_exponentToCmplx(double base, double exponent) {
    struct C *result = malloc(sizeof(struct C));
    result->refCounter = 0;
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

uint32_t math_usum(struct UIntArray *nums) {
    uint32_t result = 0;
    for (int i = 0; i < nums->length; i++) {
        result += nums->content[i];
    }
    return result;
}

uint32_t math_uprod(struct UIntArray *nums) {
    uint32_t result = 1;
    for (int i = 0; i < nums->length; i++) {
        result *= nums->content[i];
    }
    return result;
}

int32_t math_sum(struct IntArray *nums) {
    int32_t result = 0;
    for (int i = 0; i < nums->length; i++) {
            result += nums->content[i];
    }
    return result;
}

int32_t math_prod(struct IntArray *nums) {
    int32_t result = 1;
    for (int i = 0; i < nums->length; i++) {
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

uint32_t math_arrayGCD(struct UIntArray *nums) {
    uint32_t len = nums->length;
    if (len == 0) return 0;
    uint32_t gcd = nums->content[0];
    for (uint32_t i = 1; i < len; i++) {
        gcd = math_GCD(gcd, nums->content[i]);
    }
    return gcd;
}

uint32_t math_LCM(uint32_t n, uint32_t m) {
    return n * m / math_GCD(n, m);
}

uint32_t math_arrayLCM(struct UIntArray *nums) {
    return math_uprod(nums) / math_arrayGCD(nums);
}

double math_getE() {
    return M_E;
}

_Bool math_isNaN32(float val) {
    return isnan(val) != 0;
}

_Bool math_isFinite32(float val) {
    return isfinite(val) != 0;
}

_Bool math_isInfinite32(float val) {
    return isinf(val) != 0;
}

_Bool math_isNaN64(double val) {
    return isnan(val) != 0;
}

_Bool math_isFinite64(double val) {
    return isfinite(val) != 0;
}

_Bool math_isInfinite64(double val) {
    return isinf(val) != 0;
}

int math_sign(double val) {
    if (val == 0) {
        return 0;
    } else if (val < 0) {
        return -1;
    } else {
        return 1;
    }
}

int math_isign(int val) {
    if (val == 0) {
        return 0;
    } else if (val < 0) {
        return -1;
    } else {
        return 1;
    }
}

int math_icopysign(int magnitude, int direction) {
    return abs(magnitude) * math_isign(direction);
}

// sin, cos, tan, asin, acos, atan, atan2, sinh, cosh, tanh, asinh, acosh, atanh, sign, copysign are all llvm intrinsics or c builtins
