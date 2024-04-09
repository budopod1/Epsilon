# Epsilon Documentation

## Table of Content

1. [Syntax](/docs/syntax.md)
1. [Modules & CLI](/docs/modules.md)
1. [Module Refrence](/docs/modref.md)

## Module Reference

Epsilon currently has one module in its standard library.

### math

The `math` library contains several common mathematical functions, as well as complex numbers.

#### Structs

**C**<br>
Usage: represents a complex number<br>
`Q`: `real`<br>
`Q`: `imag`

#### Functions

**ln**<br>
Usage: Computes the natural log<br>
`Q#ln<argument:Q>`

**log**<br>
Usage: Computes the logarithm in the requested base<br>
`Q#log<argument:Q><base:Q>`

**complex addition**<br>
Usage: Perform addition between complex numbers<br>
`Q#<a:C> + <b:C>`

**complex subtraction**<br>
Usage: Perform subtraction between complex numbers<br>
`Q#<a:C> - <b:C>`

**complex multiplication**<br>
Usage: Perform multiplication between complex numbers<br>
`Q#<a:C> * <b:C>`

**complex division**<br>
Usage: Perform division between complex numbers<br>
`Q#<a:C> / <b:C>`

**complex $exp$**<br>
Usage: Computes $exp(z)$ for $z\in\mathbb{C}$<br>
`C#exp<val:C>`

**$exp$**<br>
Usage: Computes $exp(x)$<br>
`Q#exp<val:Q>`

**sum**<br>
Usage: Computes the sum of a list of integers<br>
`Z#sum<nums:[Z]>`

**unsigned sum**<br>
Usage: Computes the sum of a list of unsigned integers<br>
`W#sum<nums:[W]>`

**product**<br>
Usage: Computes the product of a list of integers<br>
`Z#prod<nums:[Z]>`

**unsigned product**<br>
Usage: Computes the product of a list of unsigned integers<br>
`W#prod<nums:[W]>`

**GCD**<br>
Usage: Computes the GCD of two numbers<br>
`W#GCD<a:W><b:W>`

**Array GCD**<br>
Usage: Computes the GCD of an array<br>
`W#GCD<nums:[W]>`

**LCM**<br>
Usage: Computes the LCM of two numbers<br>
`W#LCM<a:W><b:W>`

**Array LCM**<br>
Usage: Computes the LCM of an array<br>
`W#LCM<nums:[W]>`

**$e$**<br>
Usage: Returns $e$'s value<br>
`Q#e`

**is NaN**<br>
Usage: Determines if the specified float is `NaN`<br>
`Bool#is_NaN<val:T>` where `T` is `Q64` or `Q32`

**is finite**<br>
Usage: Determines if the specified float is *not* $\pm\infty$ or `NaN`<br>
`Bool#is_finite<val:T>` where `T` is `Q64` or `Q32`

**is infinite**<br>
Usage: Determines if the specified float is $\pm\infty$<br>
`Bool#is_infinite<val:T>` where `T` is `Q64` or `Q32`

**Trig**<br>
All basic trig functions follow the pattern `Q#sin<Q:val>`, where `sin` is replaced with one of:<br>
`sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, or `atanh`.

**atan2**<br>
Usage: Computes the angle clockwise off the $+x$ axis in the range $(-\pi,\pi]$ to the specified point from the origin<br>
`Q#atan2<y:Q><x:Q>`

**sign**<br>
Usage: Returns -1 if `val`<0, 1 if `val`>0, and 0 otherwise<br>
`Z#<val:T>.sign` where `T` is `Z` or `Q`

**copysign**<br>
Usage: Returns $|x|\cdot\text{sign}(y)$<br>
`T#<x:T>.copysign<y:T>` where `T` is `Z` or `Q`
