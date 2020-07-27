namespace MyNamespace
{
	class[public] Class
	{
		var[public] int Integer;
		method[public] entrypoint void main()
		{
			var int thingy = Multiply(5,3);
			if(thingy == null)
			{
				thingy++;
				return;
			}
		}
		method int Multiply(var int numOne, var int numTwo)
		{
			var Class other = new Class();
			return numOne * numTwo * other.Integer;
		}		
	}
	class OtherClass
	{
		var[public] int number;
		method[public] bool IsTrue()
		{
			return true;
		}
	}
}