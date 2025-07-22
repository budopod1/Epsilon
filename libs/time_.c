#ifdef _WIN32
#include <syncapi.h>
#include <sysinfoapi.h>
#include <profileapi.h>
#else
#include <time.h>
#endif

#include <math.h>
#include "builtins.h"

#define ERR_START "FATAL ERROR IN time: "

#define NANO_SIZE 1000000000.0
#define WIN_TICK_SIZE 10000000.0

void time_sleep_for_seconds(double duration) {
    if (duration < 0) {
        epsl_panicf(
            ERR_START "Cannot sleep for a negative amount of time"
        );
    }

#ifdef _WIN32
    Sleep((DWORD)(duration * 1000));
#else // _WIN32
    struct timespec remaining = {
        (time_t)duration, // seconds
        (long)(fmod(duration, 1) * NANO_SIZE) // nanoseconds
    };
    while (nanosleep(&remaining, &remaining) != 0);
#endif
}

double time_get_unix_timestamp() {
#ifdef _WIN32
    FILETIME filetime;
    GetSystemTimePreciseAsFileTime(&filetime);
    uint64_t ticks = filetime.dwLowDateTime + (filetime.dwHighDateTime << 32);
    // constant is sourced from:
    // https://github.com/python/cpython/blob/4b540313238de9d53bd9d9866eb481e954ad508f/Python/pytime.c#L916
    uint64_t ticks_since_epoch = ticks - 116444736000000000;
    return ticks_since_epoch / WIN_TICK_SIZE;
#else
    struct timespec time = {0};
    clock_gettime(CLOCK_REALTIME, &time);
    return time.tv_sec + time.tv_nsec / NANO_SIZE;
#endif
}

#ifdef _WIN32
static LARGE_INTEGER perf_frequency = {0};
#endif

double time_get_perf_counter() {
#ifdef _WIN32
    if (perf_frequency.QuadPart == 0) {
        QueryPerformanceFrequency(&perf_frequency);
    }

    LARGE_INTEGER ticks;
    QueryPerformanceCounter(&ticks);

    return (double)ticks.QuadPart / perf_frequency.QuadPart;
#else
    struct timespec time = {0};
    clock_gettime(CLOCK_MONOTONIC, &time);
    return time.tv_sec + time.tv_nsec / NANO_SIZE;
#endif
}
