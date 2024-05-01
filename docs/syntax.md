# Epsilon Documentation

## Table of Content

1. [Syntax](/docs/syntax.md)
1. [Modules & CLI](/docs/modules.md)
1. [Module Refrence](/docs/modref.md)

## Syntax

### Note

This Epsilon compiler is currently slightly noncomformant.
* The .unwrap function produces undefined behaviour when called on null
* Indexing an array out of bounds produces undefined behaviour

### Types

Epsilon is a strongly typed language, meaning all expressions have defined types at compile time. There are two kinds of types in Epsilon: reference types and value types. Value types are types whose values are stored locally on the stack, meaning that they must be copied when passed to or returned from functions. Reference types, however, have their data stored on the heap, so the stack only stores a pointer to the location of the data on the heap. As such, reference types must only copy the memory address of the data, instead of the data itself, when the data is moved around. This however comes with the downside that data stored on the heap is slower to access. Most types that store only a small amount of data&mdash;such as number types&mdash;are value types, while larger types&mdash;such as arrays&mdash;are reference types.

#### Bit Types & Generic Types

Some types have a varying width in memory, so you can suffix the type name with a number specifying the number of bits that should be used for the type. Some types take generics, and these generics can be specified with a syntax of the form `BaseType<T1, T2, etc>`.

#### Unsigned Integer Types

There are four types of unsigned integers in Epsilon: `W`, `Byte`, `Bool`, and `L`. `W` (the name comes from the set of whole numbers, which contains all integers >= 0) is the general unsigned integer type, and is a bit type, with a default bit count of 32. `Byte` is equivalent in all but name to `W8` and represents a byte of data. `Bool` is equivalent to `W1`. The final unsigned integer type, `L` (the name comes from `long`) is an alias for `W64`.

#### The Signed Integer Type

There is only one signed integer type in Epsilon, `Z` (the name comes from ℤ, the symbol for the set of integers). `Z` is a bit type, with a default bit count of 32.

#### The Float Type

There is also only one floating point type, `Q`. `Q` is a bit type, but only bit counts of 16, 32, or 64 (the default) are guaranteed to be supported.

#### Array Types

The type `Array` in Epsilon is a generic type that takes one generic. It stores a variable-sized array on the heap, and is a reference type. `Str` is an alias for `Array<Byte>`.

#### The Optional Type

The type `Optional` in Epsilon is a generic type that takes one generic, and that generic must be a reference type. It stores a reference or null, and is itself a reference type.

#### The File Type

`File` in Epsilon stores a reference to a file on disk or null. It is a reference type.

#### Struct Types

Your programs in Epsilon can define structs, instances of which have the type of the struct itself. Struct types are always reference types.

#### Type Shortcuts

Epsilon contains two type shortcuts, which are shorter syntaxes for generic types:
* `[T]` -> `Array<T>`
* `T?` -> `Optional<T>`


#### Implicit Type Conversion

Some number types can be converted to each other. The conversions are transitive.
* `Bool` -> `Byte`
* `Byte` -> `W`
* `W` -> `Z`
* `Z` -> `Q`

For all reference types `T`, `T` is convertible to `Optional<T>` (but not the other way around).

### Structs

The only user-defined data structure in Epsilon is the struct. Structs can be created at the top level of your Epsilon program with the syntax:

    StructName {
        Field1Type:field_1_name,
        Field2Type:field_2_name,
        etc
    }

The struct can later be referenced as a type simply by the struct name.

### Functions

All code in Epsilon is contained within functions. The basic function syntax is:

    ReturnType#FuncTemplate {
        // Epsilon code goes here
    }

If the function returns void the return type can be omitted.

#### Function Templates

Function templates dictate the way the function can be called. They can consist of literal text and symbols that must be matched at the call site, as well as function parameters. Function parameters can be specified with the syntax `<ParamType:param_name>` Here are some examples of functions and how they can be called:

    #foo {...}

Can be called by `foo`
<br><br>

    #do_something<Z:number> {...}

Can be called by `do_something[3]`
<br><br>

    #<Z:num1> + <Z:num2> {...}

Can be called by `[1] + [5]`
<br><br>

    #output_numbers<Z:a><Z:b> {...}

Can be called by `output_numbers[1][2]`

#### The Main Function

Every standalone Epsilon program must have one and only one `main` function. The `main` function's return type must be `Z`, and its template must be simply `main`.

### Comments

There are two kinds of comments: single-line comments and multiline comments.

Single-line comments start with `//` and continue until the end of the line.

Multiline comments start with `/*` and end at the next `*/`.

### Code Blocks

Code blocks start with `{` and end with `}`. They consist of a series of lines. Each line must end with `;`.

### Literals

Literals are values defined exactly in the source code. Unsigned integer literals are written simply as a number, such as `0` or `100`. Positive float literals are just as simple. `1.2` and `3.1415926` are both float literals. Negative number literals do not exist in Epsilon: instead negative numbers are written by prefixing positive number literals with the negation operator. In other words, you write negative numbers just as you would expect.

String literals are text enclosed by `"`s, and they have a type of `Str` aka `Array<Byte>`. There are several valid escape sequences in strings:

| Escape Sequence | Resulting Character |
|:---------------:| ------------------- |
| `\n`            | new line            |
| `\t`            | tab                 |
| `\r`            | return              |
| `\\`            | `\` (backslash)     |
| `\0`            | null character      |
| `\a`            | BEL character       |
| `\b`            | backspace character |
| `\v`            | vertical tab        |
| `\f`            | form field          |
| `\"`            | `"` (quote)         |

Epsilon also has byte constants, which are single characters enclosed by `'`s. The same escape sequences as for strings apply, except for the last escape sequence which is replaced with `\'` -> `'`.

Epsilon also supports fraction literals, so `1/3` (but not `-1/3`) is considered a single number (assuming the order of operations allows it to be one).

### Arithmetic

Epsilon supports all basic arithmetic operators: `+` (addition), `-` (subtraction and negation), `*` (multiplication), `/` (division), and `%` (modulo). Both operands either have to be the same type or one has to be convertible to the type of the other. The result type of the operation will be the highest type between the two. The one exception to this rule is division, wherein the result type will always be `Q`. If you require the division to have an integer result, you can use the integer division operator, `~~/`.

Epsilon also supports exponentiation with the `**` operator. The result of exponentiation will be an integer if the base is an integer and the exponent is an unsigned integer. Otherwise, the result will be a float. Exponentiation with an exponent of a constant integer is optimized to a series of multiplications (and if the integer is negative, a division). When the exponent is a literal equal to ½ or ⅓, a direct call to a square root or cube root function is used.

Parentheses work normally in Epsilon.

### Booleans

#### Truthy Values

All values are either truthy or falsy. A truthy value is a value that is considered true when used in a boolean context (eg. an `if` statement), and a falsy one is one that would be considered false. Arrays and structs are always truthy. Numbers that don't equal 0 are considered truthy. Optionals and Files are truthy if they don't contain null.

#### Boolean Operations

Boolean operations can take parameters of any type, and the truthy values will be treated as true, while the falsy values will be treated as false. Epsilon boolean operators are shown in the table below:

| Operator | Operation        |
|:-------- | ---------------- |
| `!`      | Logical Negation |
| `&&`     | Logical And      |
| `\|\|`   | Logical Or       |
| `^^`     | Logical XOR      |

### Bitwise Operations

The result of a bitwise operation always has the type of its first operand, as such `a & b` is not equivalent to `b & a`. Here's the list of bitwise operations:

| Operator | Operation        |
|:-------- | ---------------- |
| `<<`     | Bitshift left    |
| `>>`     | Bitshift right   |
| `~`      | Bitwise negation |
| `&`      | Bitwise AND      |
| `\|`     | Bitwise OR       |
| `^`      | Bitwise XOR      |

### Comparison

#### Equality

The equality operator, `==`, checks that the two supplied values have equal value if they are of a value type, and that they are the same values if they are of a reference type.

The inequality operator, `!=`, is the opposite of the equality operator. In fact, `a != b` iff `!(a == b)`.

#### Range

There are four range comparison operators: `<` (less than), `>` (greater than), `<=` (less than or equal to), and `>=` (greater than or equal to). They can only be applied to numbers.

### Keywords & Constants

Epsilon has several reserved keywords: `return`, `if`, `else`, `elif`, `while`, `switch`, `break`, `continue`, `null`, and `for`.

Here are the constants included in Epsilon:

1. `true`
2. `false`
3. `infinity`
4. `NaN`
5. `pi`

Constants are also considered reserved words.

### Creating Arrays & Structs

Structs can be instantiated with the syntax `StructName [field1Value, field2Value, etc]`. Arrays use a similar syntax: `ArrayType [value1, value2]` so both of these are valid:

    [Z] []; // empty array of integers
    Array<W> [1, 2]; // array of unsigned integers

### Variables & Scoping

Variables can be declared with the syntax `VarType:varName`. They will be available in the scope enclosing the declaration and any scopes within that scope. Only value types can be declared without a value, so:

    // This is legal:
    Z:number;
    // This is not:
    [Z]:numbers;

You can assign to variables with the `=` operator. A variable declaration and assignment can (and must for reference types) be combined into one statement.

    // All four of these are legal
    Z:number = 3;
    [Z]:numbers = [Z] [1, 2, 4];
    number = 5;
    numbers = [Z] [6, 7, 8];

### Array & Struct Access

Arrays can be accessed with the syntax `array[index]`, while struct values can be accessed via `struct.fieldName`. Both of these are assignable values so:

    // These are valid:
    numbers[1] = 0;
    someStruct.fieldName = 10;

Accessing an index out of bounds of the array will result in an invalid memory access.

### Control Flow

The code in an `if` statement will only be executed if the condition is truthy.

    if (condition) {
        /* statements go here */
    };

Note that control flow code blocks end with a `;`.

A `while` loop's block is executed repeatedly while the condition is truthy:

    while (condition) {
        /* statements go here */
    };

A `switch` statement compares the value given to it to many constant cases:

    switch (value) 
        (case1) {
            /* statements */
        }
        (case2) {
            /* statements */
        };

`switch` statements can also have default blocks which flow will transfer to if the value does not match any case:

    switch (value)
        (case1) {
            /* statements */
        }
        ...
        {
            /* default statements */
        };

`elif` clauses can be added to the end of an `if` statement to form a more complex conditional:

    if (condition1) {
        /* statements executed if condition1 is truthy */
    } elif (condition2) {
        /* statements executed if condition2 is truthy but not condition1 */
    };

An `else` clause can be added to the end of the conditional:

    if (condition1) {
        /* statements executed if condition1 is truthy */
    } elif (condition2) {
        /* statements executed if condition2 is truthy but condition1 is falsy */
    } else {
        /* statements executed if all above conditions are falsy */
    };

The most complex flow control structure is the `for` statement. The basic syntax is:

    for (VarType:varName clause1 clause2 etc) {
        /* statements in for loop */
    };

A `for` clause consists of the clause name followed by the clause's value. There are five types of `for` clauses:

1. `to`: one more than the highest value the loop should go to.
2. `from`: the smallest value of the loop should go to. The default is 0.
3. `step`: the step between values in the loop. If the step is negative, the loop will start from the top instead. The default is 1.
4. `in`: takes an array which the `for` loop variable will iterate over. `to` is given a default value of the end of the array.
5. `enumerating`: takes an array and sets the `to` value to the end of the array.

A `for` loop must have a `to`, `in`, or `enumerating` clause. It cannot have both an `enumerating` clause and a `to` or `in` clause.

Some examples of `for` loops:

    for (Z:i to 6) {
        // i goes [0, 6)
    };
    for (Z:i from 2 to 6) {
        // i goes [2, 6)
    };
    for (Z:i from 2 to 6 step -2) {
        // -5, -3
    };
    for (Z:i in [Z] [9, 5, 7]) {
        // 9, 5, 7
    };
    for (Z:i enumerating [Z] [9, 5, 7]) {
        // 0, 1, 2
    };

The `for` loop variable is available in the scope of the loop's code block, but not in the scope enclosing the `for` statement itself.

### Pre & Post Increment & Decrement

|           | Pre | Post |
| --------- | --- | ---- |
| Increment | Increments the referenced value and returns the new value: `++x` | Increments the referenced value and returns the old value: `x++` |
| Decrement | Decrements the referenced value and returns the new value: `--x` | Decrements the referenced value and returns the old value: `x--` |

    Z:x = 3;
    Z:y = x++;
    // x now equals 4, while y equals 3

### Compound Assignments

The operators `+`, `-`, `*`, `/`, `%`, `&`, `|`, and `^` have compound assignment forms. In a compound assignment, the operation specified is performed on both the current value of the assignable and the value supplied to the compound assignment, and the resulting value is stored back into the assignable value.

    [Z]:nums = [Z] [1, 2, 3];
    nums[1] *= 2;
    // nums now holds the values: [1, 4, 3]

Variables, array accesses, and struct field accesses are all assignable values.

### The Cast Operator

The cast operator, which uses the syntax `(NewType)value`, will cast `value` to the type `NewType`, if it is possible.

### Continue, Break, and Return

The `continue` keyword will jump to the next iteration of the loop enclosing the statement. 

The `break` keyword will exit the loop enclosing the statement.

The `return` keyword exits the function in which it is in. If the function has a void return type, then the `return` keyword must be used as a complete line. If the function has a non-void return type the `return` must be followed by the value to be returned from the function.

### Builtin Functions

There are currently 69 builtin functions:

**#1: length**<br>
Usage: TODO<br>
`W64#<array:Array<Any>>.length`

**#2: capacity**<br>
Usage: TODO<br>
`W64#<array:Array<Any>>.capacity`

**#3: append**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.append<value:Any>`

**#4: require_capacity**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.require_capacity<value:W64>`

**#5: shrink_mem**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.shrink_mem`

**#6: pop**<br>
Usage: TODO<br>
`T#<array:Array<T>>.pop<index:W64>` where `T` is `Any`

**#7: insert**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.insert<index:W64><value:Any>`

**#8: clone**<br>
Usage: TODO<br>
`Array<T>#<array:Array<T>>.clone` where `T` is `Any`

**#9: extend**<br>
Usage: TODO<br>
`Void#<array:Array<T>>.extend<array2:Array<T>>` where `T` is `Any`

**#10: concat**<br>
Usage: TODO<br>
`Array<T>#<array1:Array<T>>.concat<array2:Array<T>>` where `T` is `Any`

**#11: make_range_array**<br>
Usage: TODO<br>
`Array<Z>#make_range_array<end:Z>`

**#12: make_range_array**<br>
Usage: TODO<br>
`Array<Z>#make_range_array<start:Z><end:Z>`

**#13: make_range_array**<br>
Usage: TODO<br>
`Array<Z>#make_range_array<start:Z><end:Z><step:Z>`

**#14: abs**<br>
Usage: TODO<br>
`W#<value:Z>.abs`

**#15: abs**<br>
Usage: TODO<br>
`Q64#<value:Q64>.abs`

**#16: concat**<br>
Usage: TODO<br>
`Array<T>#<Array<T>> + <Array<T>>` where `T` is `Any`

**#17: stringify**<br>
Usage: TODO<br>
`Str#<value:Any>.stringify`

**#18: print**<br>
Usage: TODO<br>
`Void#print<value:Any>`

**#19: println**<br>
Usage: TODO<br>
`Void#println<value:Any>`

**#20: left_pad**<br>
Usage: TODO<br>
`Void#<str:Str>.left_pad<len:W64><chr:Byte>`

**#21: right_pad**<br>
Usage: TODO<br>
`Void#<str:Str>.right_pad<len:W64><chr:Byte>`

**#22: slice**<br>
Usage: TODO<br>
`Array<T>#<arr:Array<T>>.slice<start:W64><end:W64>` where `T` is `Any`

**#23: count_chr**<br>
Usage: TODO<br>
`W64#<str:Str>.count_chr<chr:Byte>`

**#24: count**<br>
Usage: TODO<br>
`W64#<arr:Array<T>>.count<sub:Array<T>>` where `T` is `Any`

**#25: overlap_count**<br>
Usage: TODO<br>
`W64#<arr:Array<T>>.overlap_count<sub:Array<T>>` where `T` is `Any`

**#26: nest**<br>
Usage: TODO<br>
`Array<Array<T>>#<arr:Array<T>>.nest` where `T` is `Any`

**#27: split**<br>
Usage: TODO<br>
`Array<Array<T>>#<arr:Array<T>>.split<sub:Array<T>>` where `T` is `Any`

**#28: starts_with**<br>
Usage: TODO<br>
`Bool#<arr:Array<T>>.starts_with<sub:Array<T>>` where `T` is `Any`

**#29: ends_with**<br>
Usage: TODO<br>
`Bool#<arr:Array<T>>.ends_with<sub:Array<T>>` where `T` is `Any`

**#30: equals**<br>
Usage: TODO<br>
`Bool#<v1:T> equals <v2:T>` where `T` is `Any`

**#31: not equals**<br>
Usage: TODO<br>
`Bool#not<v1:T> not equals <v2:T>` where `T` is `Any`

**#32: equals depth**<br>
Usage: TODO<br>
`Bool#<v1:T> equals <v2:T> depth <depth:W>` where `T` is `Any`

**#33: not equals depth**<br>
Usage: TODO<br>
`Bool#<v1:T> not equals <v2:T> depth <depth:W>` where `T` is `Any`

**#34: join**<br>
Usage: TODO<br>
`Array<T>#<arr:Array<Array<T>>>.join<sep:Array<T>>` where `T` is `Any`

**#35: index_of**<br>
Usage: TODO<br>
`W64#<arr:Array<T>>.index_of<elem:T>` where `T` is `Any`

**#36: index_of_subsection**<br>
Usage: TODO<br>
`W64#<arr:Array<T>>.index_of_subsection<sub:Array<T>>` where `T` is `Any`

**#37: parse_int**<br>
Usage: TODO<br>
`Z32#parse_int<str:Str>`

**#38: is_valid_parsed_int**<br>
Usage: TODO<br>
`Bool#is_valid_parsed_int<int:Z>`

**#39: parse_float**<br>
Usage: TODO<br>
`Q32#parse_float<str:Str>`

**#40: is_valid_parsed_float**<br>
Usage: TODO<br>
`Bool#is_valid_parsed_float<int:Q32>`

**#41: read_input_line**<br>
Usage: TODO<br>
`Str#read_input_line`

**#42: open_file**<br>
Usage: TODO<br>
`File#open_file<str:Str><mode:Z>`

**#43: FILE_READ_MODE**<br>
Usage: TODO<br>
`Z32#FILE_READ_MODE`

**#44: FILE_WRITE_MODE**<br>
Usage: TODO<br>
`Z32#FILE_WRITE_MODE`

**#45: FILE_APPEND_MODE**<br>
Usage: TODO<br>
`Z32#FILE_APPEND_MODE`

**#46: FILE_BINARY_MODE**<br>
Usage: TODO<br>
`Z32#FILE_BINARY_MODE`

**#47: is_open**<br>
Usage: TODO<br>
`Bool#<file:File>.is_open`

**#48: mode**<br>
Usage: TODO<br>
`Z32#<file:File>.mode`

**#49: close**<br>
Usage: TODO<br>
`Void#<file:File>.close`

**#50: length**<br>
Usage: TODO<br>
`Z64#<file:File>.length`

**#51: pos**<br>
Usage: TODO<br>
`Z64#<file:File>.pos`

**#52: read_all**<br>
Usage: TODO<br>
`Optional<Str>#<file:File>.read_all`

**#53: read_some**<br>
Usage: TODO<br>
`Optional<Str>#<file:File>.read_some<bytes:W64>`

**#54: set_pos**<br>
Usage: TODO<br>
`Bool#<file:File>.set_pos<pos:W64>`

**#55: jump_pos**<br>
Usage: TODO<br>
`Bool#<file:File>.jump_pos<amount:W64>`

**#56: read_line**<br>
Usage: TODO<br>
`Optional<Str>#<file:File>.read_line`

**#57: read_line_reached_EOF**<br>
Usage: TODO<br>
`Bool#read_line_reached_EOF`

**#58: read_lines**<br>
Usage: TODO<br>
`Optional<Array<Str>>#<file:File>.read_lines`

**#59: write**<br>
Usage: TODO<br>
`Bool#<file:File>.write<content:Str>`

**#60: is_null**<br>
Usage: TODO<br>
`Bool#<nullable:Any>.is_null`

**#61: unwrap**<br>
Usage: TODO<br>
`T#<optional:Optional<T>>.unwrap` where `T` is `Any`

**#62: abort**<br>
Usage: TODO<br>
`Void#abort<string:Str>`

**#63: blank_from_type**<br>
Usage: TODO<br>
`Array<T>#<array:Array<T>>.blank_from_type<amount:W64>` where `T` is `Any`

**#64: unique**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.unique`

**#65: sort**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.sort`

**#66: sort_inverted**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.sort_inverted`

**#67: dedup**<br>
Usage: TODO<br>
`Void#<array:Array<Any>>.dedup`

**#68: repeat**<br>
Usage: TODO<br>
`Array<T>#<array:Array<T>>.repeat<times:W64>` where `T` is `Any`

**#69: truthy**<br>
Usage: determines whether the specified value is truthy<br>
`Bool#<value:Any>.truthy`

**#70: floor**<br>
Usage: computes the largest integer less than or equal to the given value.<br>
`Z#<num:Q>.floor`

**#71: ceil**<br>
Usage: computes the smallest integer greater than or equal to the given value.<br>
`Z#<num:Q>.ceil`

**#72: round**<br>
Usage: computes the integer with the smallest absolute distance from the given value.<br>
`Z#<num:Q>.round`
