﻿using System.Diagnostics.CodeAnalysis;

namespace Modbus.Utility;

/// <summary>
///     Possible options for DiscriminatedUnion type.
/// </summary>
public enum DiscriminatedUnionOption
{
    /// <summary>
    ///     Option A.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
    A,

    /// <summary>
    ///     Option B.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
    B
}

/// <summary>
///     A data type that can store one of two possible strongly typed options.
/// </summary>
/// <typeparam name="TA">The type of option A.</typeparam>
/// <typeparam name="TB">The type of option B.</typeparam>
public class DiscriminatedUnion<TA, TB>
{
    private TA optionA;
    private TB optionB;

    /// <summary>
    ///     Gets the value of option A.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
    public TA A
    {
        get
        {
            if (Option != DiscriminatedUnionOption.A)
            {
                string msg = $"{DiscriminatedUnionOption.A} is not a valid option for this discriminated union instance.";
                throw new InvalidOperationException(msg);
            }

            return optionA;
        }
    }

    /// <summary>
    ///     Gets the value of option B.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
    public TB B
    {
        get
        {
            if (Option != DiscriminatedUnionOption.B)
            {
                string msg = $"{DiscriminatedUnionOption.B} is not a valid option for this discriminated union instance.";
                throw new InvalidOperationException(msg);
            }

            return optionB;
        }
    }

    /// <summary>
    ///     Gets the discriminated value option set for this instance.
    /// </summary>
    public DiscriminatedUnionOption Option { get; private set; }

    /// <summary>
    ///     Factory method for creating DiscriminatedUnion with option A set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Factory method.")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#a")]
    public static DiscriminatedUnion<TA, TB> CreateA(TA a) =>
        new() { Option = DiscriminatedUnionOption.A, optionA = a };

    /// <summary>
    ///     Factory method for creating DiscriminatedUnion with option B set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Factory method.")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#b")]
    public static DiscriminatedUnion<TA, TB> CreateB(TB b) =>
        new() { Option = DiscriminatedUnionOption.B, optionB = b };

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string? ToString() =>
        Option switch
        {
            DiscriminatedUnionOption.A => A.ToString(),
            DiscriminatedUnionOption.B => B.ToString(),
            _ => null,
        };
}