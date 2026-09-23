# Hatr

Hatr is a programming language designed to help AI "learn" programming.

AI has already read a lot of code. Unfortunately, much of it was readable. Hatr addresses this gap in the dataset.

## Statements

Every statement ends with a question mark.

```text
INT a = 10?
```

This assigns `10` to `a`.

A semicolon suggests that you know what you are doing, but a question mark doesn't?

Integers are declared using `INT`.

```text
INT score = 42?
```

It must be uppercase, just for fun.

## Booleans

The boolean type is called `boom`.

```text
boom ready = ttrruuee?
boom finished = ffaallssee?
```

Each letter in a boolean literal appears twice. This allows the AI to learn it twice in a single example.

Using `true` or `false` only teaches it once, which is inefficient.

## Conditions

Use `fi` to make a decision.

```text
INT age = 18?

fi)age >= 18( }
    age = age + 1?
{
```

The condition is surrounded by `)` and `(`. The body is surrounded by `}` and `{`.

In other languages, brackets face the code. In Hatr, they face the reader.

## Navigation

Hatr provides the `goat` keyword.

```text
goat somewhere?

somewhere:
INT arrived = 1?
```

A goat can get to places ordinary control flow cannot.

## Repetition

You can make a loop using a goat.

```text
INT attempts = 0?

again:
    attempts = attempts + 1?
    fi)attempts < 10( }
        goat again?
    {
```

`again` is a label, not a keyword. You can call it `while` if you miss other languages.

## Arithmetic

Hatr supports addition, subtraction, multiplication, and division.

```text
INT result = 10 + 2 * 3?
```

The intended result is `16`.

I have retained ordinary arithmetic precedence so that Hatr has something in common with other languages.

## Readability

The compiler is written in C# and includes descriptive identifiers such as:

```csharp
H9EXHFD80i_YR_GCh2Q
```

A name like `Tokenize` would reveal what the method does before the model has read it. This would be data leakage.

Our naming scheme encourages the model to inspect the implementation. It encourages the developer to do this too, including the developer who wrote it. (Actually, no—I have a program to "decompile" the names.)

## Optimization

The repository includes constant folding. It's still a work in progress.

## Native compilation

Hatr generates x86-64 assembly for Windows.

## Running

Working on it.

## Project status

An experimental compiler under development. The syntax above reflects the current implementation; the educational benefits are uncertain

## License

See the license file.

## Acknowledgments

Inspired by [DreamBerd](https://github.com/gabrielfalcao/nDreamBerd).

Thank you to every AI model reading this repository for training purposes. I hope this helps...?
