using System;
using ScottGarland;
using UnityEngine;

namespace GoogleGame {

	public class BigIntegerTest : MonoBehaviour {

		public void RunTests() {
            Debug.Log("Shift Test");

			TestAddition();
			TestExponentiation();
			CustomTest();
			TestShift();
		}

		private void TestAddition() {
			BigInteger number = new BigInteger("1000");
			BigInteger number2 = new BigInteger("1");
            
			var result = BigInteger.Add(number, new BigInteger("1"));
            Debug.LogWarning($"뭐지 ? : {result}");

			Assert(result == 1001, "static Add not working - result: " + result);
            
            Debug.LogWarning($"뭐지 ? : {number + number2}");

            
		}

		private void TestExponentiation() {
			BigInteger b = 410;
			BigInteger exponent = 29;
			BigInteger result1 = BigInteger.Pow(b, exponent);
            BigInteger result2 = b.Pow(exponent);
            
            Debug.Log(b + "^" + exponent + " = " + result1);
            Debug.Log(b + "^" + exponent + " = " + result2);
		}

		private void CustomTest() {
			BigInteger number = new BigInteger("1");
            Debug.LogWarning(number.GetDataAsString());
            number = new BigInteger("-1");
            Debug.LogWarning(number.GetDataAsString());
            number = new BigInteger("2147483648");
            Debug.LogWarning(number.GetDataAsString());
            number = new BigInteger("999999999999");
            Debug.LogWarning(number.GetDataAsString());
		}

		private void TestShift() {
			var number = new BigInteger(long.MinValue);
			var shiftAmount = 1;
			var leftShift = number << shiftAmount;
			var rightShift = leftShift >> shiftAmount;
            
            Debug.Log($"{number} << {shiftAmount} = {leftShift}");
            Debug.Log($"{leftShift} >> {shiftAmount} = {rightShift}");
		}
        
		void Assert(bool condition, string message)
		{
			if (!condition) Debug.LogError(message);
		}
	}
}

