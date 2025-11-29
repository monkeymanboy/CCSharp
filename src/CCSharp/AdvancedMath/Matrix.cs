using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

public class Matrix
{
    /// <summary>
    /// Represents a single row in the Matrix.
    /// </summary>
    class Row
    {
        /// <summary>
        /// Gets or sets the value at the specified column in this row.
        /// </summary>
        /// <param name="col">The column index (1-based).</param>
        /// <returns>The value at the specified column.</returns>
        public double this[int col]
        {
            get => default;
            set { }
        }
    }

    [LuaProperty("rows")]
    public int Rows { get; set; }
    [LuaProperty("columns")]
    public int Columns { get; set; }

    /// <summary>
    /// Constructs a new Matrix with the given number of rows and columns, filling it using the provided function.
    /// </summary>
    /// <param name="rows">The number of rows in the matrix.</param>
    /// <param name="columns">The number of columns in the matrix.</param>
    /// <param name="function">A function that takes in a row and column index (1-based) and returns the value at that position.</param>
    [LuaConstructor("matrix.new")]
    public Matrix(int rows, int columns, Func<int, int> function) { }

    /// <summary>
    /// Constructs a new Matrix with the given number of rows and columns, filling it with the provided value.
    /// </summary>
    /// <param name="rows">The number of rows in the matrix.</param>
    /// <param name="columns">The number of columns in the matrix.</param>
    /// <param name="fill">The value to fill the matrix with.</param>
    [LuaConstructor("matrix.new")]
    public Matrix(int rows, int columns, double fill) { }

    /// <summary>
    /// Constructs a new Matrix with the given 2D array.
    /// </summary>
    /// <param name="array">The 2D array to initialize the matrix with.</param>
    [LuaConstructor("matrix.from2DArray")]
    public Matrix(double[][] array) { }

    /// <summary>
    /// Constructs a new vector Matrix as either a single row or column/
    /// </summary>
    /// <param name="vector">The Vector to convert to a Matrix.</param>
    /// <param name="row">If true, the vector Matrix will be a single row, otherwise it will be a single column.</param>
    [LuaConstructor("matrix.fromVector")]
    public Matrix(Vector3 vector, bool row) { }

    /// <summary>
    /// Constructs a new 3x3 rotation Matrix from the given Quaternion.
    /// </summary>
    /// <param name="quaternion">The Quaternion to convert to a 3x3 rotation matrix.</param>
    [LuaConstructor("matrix.fromQuaternion")]
    public Matrix(Quaternion quaternion) { }

    /// <summary>
    /// Constructs a new identity Matrix with the given number of rows and columns.
    /// </summary>
    /// <param name="rows">The number of rows in the matrix.</param>
    /// <param name="columns">The number of columns in the matrix.</param>
    [LuaMethod("matrix.identity")]
    public Matrix(int rows, int columns) { }

    /// <summary>
    /// Solves the equation Ax = b for x, where A is a Matrix and b is a column vector.
    /// </summary>
    /// <param name="A">The matrix A.</param>
    /// <param name="b">The column vector b.</param>
    /// <param name="tolerance">The tolerance for the solution.</param>
    /// <returns>A tuple containing the solution matrix and a warning string if applicable.</returns>
    [LuaMethod("matrix.solve"), CallMethodFlags.WrapAsTable]
    public static (Matrix Solution, string Warning) Solve(Matrix A, Matrix b, double tolerance) => default;

    /// <summary>
    /// Solves the equation Ax = b for x, where A is a Matrix and b is a column vector.
    /// </summary>
    /// <param name="A">The matrix A.</param>
    /// <param name="b">The column vector b.</param>
    /// <returns>A tuple containing the solution matrix and a warning string if applicable.</returns>
    [LuaMethod("matrix.solve"), CallMethodFlags.WrapAsTable]
    public static (Matrix Solution, string Warning) Solve(Matrix A, Matrix b) => default;

    /// <summary>
    /// Gets or sets the value at the specified row.
    /// </summary>
    /// <param name="row">The row index (1-based).</param>
    /// <returns>The Row object representing the specified row.</returns>
    public Row this[int row]
    {
        get => default;
        set { }
    }

    /// <summary>
    /// Get the length (also referred to as magnitude) of this Matrix.
    /// </summary>
    /// <returns>The length of this matrix.</returns>
    [LuaMethod("length")]
    public double Length() => default;

    /// <summary>
    /// Gets the minor matrix by removing the specified row and column.
    /// </summary>
    /// <param name="row">The row to remove (1-based).</param>
    /// <param name="col">The column to remove (1-based).</param>
    /// <returns>The minor matrix.</returns>
    [LuaMethod("minor")]
    public Matrix Minor(int row, int col) => default;

    /// <summary>
    /// Calculates the determinant of the matrix.
    /// </summary>
    /// <returns>The determinant value.</returns>
    [LuaMethod("determinant")]
    public double Determinant() => default;

    /// <summary>
    /// Transposes the matrix, swapping rows and columns.
    /// </summary>
    /// <returns>The transposed matrix.</returns>
    [LuaMethod("transpose")]
    public Matrix Transpose() => default;

    /// <summary>
    /// Calculates the cofactor matrix.
    /// </summary>
    /// <returns>The cofactor matrix.</returns>
    [LuaMethod("cofactor")]
    public Matrix Cofactor() => default;

    /// <summary>
    /// Calculates the adjugate matrix.
    /// </summary>
    /// <returns>The adjugate matrix.</returns>
    [LuaMethod("adjugate")]
    public Matrix Adjugate() => default;

    /// <summary>
    /// Inverts the matrix.
    /// </summary>
    /// <returns>The inverted matrix.</returns>
    [LuaMethod("inverse")]
    public Matrix Inverse() => default;

    /// <summary>
    /// Calculates the trace of the matrix.
    /// </summary>
    /// <returns>The trace value.</returns>
    [LuaMethod("trace")]
    public double Trace() => default;

    /// <summary>
    /// Calculates the rank of the matrix.
    /// </summary>
    /// <returns>The rank value.</returns>
    [LuaMethod("rank")]
    public int Rank() => default;

    /// <summary>
    /// Gets the Frobenius norm of the matrix.
    /// </summary>
    /// <returns>The Frobenius norm value.</returns>
    [LuaMethod("frobeniusNorm")]
    public double FrobeniusNorm() => default;

    /// <summary>
    /// Gets the maximum norm of the matrix.
    /// </summary>
    /// <returns>The maximum norm value.</returns>
    [LuaMethod("maxNorm")]
    public double MaxNorm() => default;

    /// <summary>
    /// Performs the Hadamard product (element-wise multiplication) with another matrix.
    /// </summary>
    /// <param name="b">The other matrix.</param>
    /// <returns>The resulting matrix from the Hadamard product.</returns>
    [LuaMethod("hadamardProduct")]
    public Matrix HadamardProduct(Matrix b) => default;

    /// <summary>
    /// Performs element-wise division with another matrix.
    /// </summary>
    /// <param name="b">The other matrix.</param>
    /// <returns>The resulting matrix from the element-wise division.</returns>
    [LuaMethod("elementwiseDiv")]
    public Matrix ElementwiseDiv(Matrix b) => default;

    /// <summary>
    /// Checks if the matrix is symmetric.
    /// </summary>
    /// <returns>True if the matrix is symmetric, otherwise false.</returns>
    [LuaMethod("isSymmetric")]
    public bool IsSymmetric() => default;

    /// <summary>
    /// Checks if the matrix is diagonal.
    /// </summary>
    /// <returns>True if the matrix is diagonal, otherwise false.</returns>
    [LuaMethod("isDiagonal")]
    public bool IsDiagonal() => default;

    /// <summary>
    /// Checks if the matrix is an identity matrix.
    /// </summary>
    /// <returns>True if the matrix is identity, otherwise false.</returns>
    [LuaMethod("isIdentity")]
    public bool IsIdentity() => default;

    /// <summary>
    /// Creates a clone of the matrix.
    /// </summary>
    /// <returns>The cloned matrix.</returns>
    [LuaMethod("clone")]
    public Matrix Clone() => default;

    /// <summary>
    /// Performs LU Decomposition of the matrix.
    /// </summary>
    /// <returns>The L and U matrices along with the permutation array P.</returns>
    [LuaMethod("luDecomposition", CallMethodFlags.WrapAsTable)]
    public (Matrix L, Matrix U, int[] P) LuDecomposition() => default;

    /// <summary>
    /// Flattens the matrix into a one-dimensional array in row-major order.
    /// </summary>
    /// <returns>The flattened array.</returns>
    [LuaMethod("flatten")]
    public double[] Flatten() => default;

    /// <summary>
    /// Reshapes the matrix to the specified number of rows and columns.
    /// </summary>
    /// <param name="rows">The number of rows in the reshaped matrix.</param>
    /// <param name="columns">The number of columns in the reshaped matrix.</param
    /// <returns>The reshaped matrix.</returns>
    [LuaMethod("reshape")]
    public Matrix Reshape(int rows, int columns) => default;

    /// <summary>
    /// Gets a submatrix defined by the specified row and column ranges.
    /// </summary>
    /// <param name="r1">The starting row index (1-based).</param>
    /// <param name="r2">The ending row index (1-based).</param>
    /// <param name="c1">The starting column index (1-based).</param>
    /// <param name="c2">The ending column index (1-based).</param>
    /// <returns>The submatrix.</returns>
    [LuaMethod("submatrix")]
    public Matrix Submatrix(int r1, int r2, int c1, int c2) => default;

    /// <summary>
    /// Stacks another matrix vertically below this matrix.
    /// </summary>
    /// <param name="other">The matrix to stack below.</param>
    /// <returns>The resulting stacked matrix.</returns>
    [LuaMethod("vstack")]
    public Matrix Vstack(Matrix other) => default;

    /// <summary>
    /// Stacks another matrix horizontally beside this matrix.
    /// </summary>
    /// <param name="other">The matrix to stack beside.</param>
    /// <returns>The resulting stacked matrix.</returns>
    [LuaMethod("hstack")]
    public Matrix Hstack(Matrix other) => default;

    /// <summary>
    /// Gets the one norm of the matrix.
    /// </summary>
    /// <returns>The one norm value.</returns>
    [LuaMethod("oneNorm")]
    public double OneNorm() => default;

    /// <summary>
    /// Gets the two norm of the matrix.
    /// </summary>
    /// <returns>The two norm value.</returns>
    [LuaMethod("twoNorm")]
    public double TwoNorm() => default;

    /// <summary>
    /// Gets the infinity norm of the matrix.
    /// </summary>
    /// <returns>The infinity norm value.</returns>
    [LuaMethod("infinityNorm")]
    public double InfinityNorm() => default;

    /// <summary>
    /// Gets the condition number of the matrix.
    /// </summary>
    /// <returns>The condition number value.</returns>
    [LuaMethod("conditionNumber")]
    public double ConditionNumber() => default;
}