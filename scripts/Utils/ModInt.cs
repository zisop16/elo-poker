using Godot;

public class ModInt {
    private static int FixedMod(int a, int b) {
        return ((a % b) + b) % b;
    }
    public static ModInt operator +(ModInt operand) {
        return operand;
    }
    public static ModInt operator +(ModInt left, ModInt right) {
        return new ModInt(FixedMod((left.Val + right.Val), left.Base), left.Base);
    }
    public static ModInt operator +(ModInt left, int right) {
        return new ModInt(FixedMod((left.Val + right), left.Base), left.Base);
    }
    public static ModInt operator +(int left, ModInt right) {
        return right + left;
    }
    public static ModInt operator -(ModInt operand) {
        return new ModInt(FixedMod(-operand.Val, operand.Base), operand.Base);
    }
    public static ModInt operator -(ModInt left, ModInt right) {
        return left + (-right);
    }
    public static ModInt operator -(ModInt left, int right) {
        return left + (-right);
    }
    public static ModInt operator -(int left, ModInt right) {
        return (-right) + left;
    }
    public static ModInt operator ++(ModInt operand) {
        operand += 1;
        return operand;
    }
    public static ModInt operator --(ModInt operand) {
        operand -= 1;
        return operand;
    }
    public static bool operator ==(ModInt left, ModInt right) {
        return left.Val == right.Val;
    }
    public static bool operator !=(ModInt left, ModInt right) {
        return !(left.Val == right.Val);
    }
    public static bool operator ==(ModInt left, int right) {
        return left == new ModInt(right, left.Base);
    }
    public static bool operator !=(ModInt left, int right) {
        return !(left == new ModInt(right, left.Base));
    }
    public static bool operator ==(int left, ModInt right) {
        return right == left;
    }
    public static bool operator !=(int left, ModInt right) {
        return right != left;
    }

    public override string ToString() {
        return "" + Val + " mod " + Base;
    }


    private int Base;
    public int Val { get; private set; }
    public ModInt(int v, int b) {
        Base = b;
        Val = FixedMod(v, b);
    }
    public ModInt(ModInt i) {
        Base = i.Base;
        Val = i.Val;
    }

    public override bool Equals(object obj) {
        if (!(obj is int || obj is ModInt)) {
            return (ModInt)obj == this;
        }

        return false;
    }

    public override int GetHashCode() {
        return Base + Val;
    }
}