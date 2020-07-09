//Yes I know this code is does nothing useful
namespace Example
{
	class[public] AClass
	{
		var[public] int ArbitraryNum = (-)56;
		method[public] entrypoint void Main()
		{
			//Start of the method
			var string AString = "Hello";
			var char[] ACharArray = new char[](5);
			var bool IsTrue = true;
			var AnotherClass anotherClass = new AnotherClass();
			if(IsTrue)
			{
				anotherClass.NumberThree()
			}
		}
	}
	class[public] AnotherClass
	{
		method[public] int NumberThree()
		{
			return 3;
		}
	}
}