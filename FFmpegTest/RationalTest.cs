using FFmpeg.Utils;
using System.Globalization;

namespace FFmpegTest;

[TestClass]
public class RationalTest
{
    [TestMethod]
    public void Parse()
    {
        string str_rational = "30000/1001";
        string str_integer = "25";
        string str_rational_negative = "-7/3";
        string str_invalid = "invalid";
        string str_aspect_ratio = "16:9";
        string str_double = "2.5";
        string str_double_german = "2,5";

        Assert.AreEqual(new Rational(30000, 1001), Rational.Parse(str_rational));
        Assert.AreEqual(new Rational(25, 1), Rational.Parse(str_integer));
        Assert.AreEqual(new Rational(-7, 3), Rational.Parse(str_rational_negative));
        Assert.AreEqual(new Rational(16, 9), Rational.Parse(str_aspect_ratio));
        Assert.AreEqual(new Rational(5, 2), Rational.Parse(str_double));
        Assert.AreEqual(new Rational(5, 2), Rational.Parse(str_double_german, CultureInfo.GetCultureInfo("de-DE")));
        _ = Assert.Throws<FormatException>(() => Rational.Parse(str_invalid));
    }

    [TestMethod]
    public void Arithmetic()
    {
        // Define rational numbers for testing.
        Rational a = new(1, 2);
        Rational b = new(1, 3);

        // Test addition of two rational numbers.
        Assert.AreEqual(new Rational(5, 6), a + b);

        // Test subtraction of two rational numbers.
        Assert.AreEqual(new Rational(1, 6), a - b);

        // Test multiplication of two rational numbers.
        Assert.AreEqual(new Rational(1, 6), a * b);

        // Test division of two rational numbers.
        Assert.AreEqual(new Rational(3, 2), a / b);

        // Test multiplication of a rational number with a double.
        double d1 = 2.5;  // Example double value
        Rational result1 = a * d1;  // Rational * Double
        Assert.AreEqual(new Rational(5, 4), result1);  // Expect 2.5 * 1/2 = 5/4

        // Test division of a rational number by a double.
        double d2 = 2.0;  // Example double value
        Rational result2 = a / d2;  // Rational / Double
        Assert.AreEqual(new Rational(1, 4), result2);  // Expect 1/2 ÷ 2.0 = 1/4
    }


    [TestMethod]
    public void TestToString_DefaultFormat()
    {
        // Arrange
        Rational rational = new(3, 4);  // Example rational number: 3/4
        string expected = "3/4";

        // Act
        string actual = rational.ToString(null, null);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestToString_DoubleFormat()
    {
        // Arrange
        Rational rational = new(3, 4);  // Example rational number: 3/4
        string expected = "0.75";  // Should convert to double and format

        // Act
        string actual = rational.ToString("DF2", null); // "D2" means double format with 2 decimal places

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestToString_CustomIntegerFormat()
    {
        // Arrange
        Rational rational = new(3, 4);  // Example rational number: 3/4
        string expected = "3|4";  // Custom format with pipe symbol as delimiter

        // Act
        string actual = rational.ToString("|", null);  // Custom format with delimiter "|"

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestToString_CustomIntegerFormatWithProvider()
    {
        // Arrange
        Rational rational = new(123456789, 1000000);  // Example rational number: 123456789/1000000
        string expected = "123,456,789/1,000,000";  // Custom format with culture-specific number formatting

        // Act
        string actual = rational.ToString("/#,0", new System.Globalization.CultureInfo("en-US"));

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestToString_EmptyFormat()
    {
        // Arrange
        Rational rational = new(5, 7);  // Example rational number: 5/7
        string expected = "5/7";

        // Act
        string actual = rational.ToString("", null);  // No format provided, should default to "numerator/denominator"

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestToString_NullFormat()
    {
        // Arrange
        Rational rational = new(1, 2);  // Example rational number: 1/2
        string expected = "1/2";

        // Act
        string actual = rational.ToString(null, null);  // Null format provided, should default to "numerator/denominator"

        // Assert
        Assert.AreEqual(expected, actual);
    }


}
