namespace MyNamespace
{
	class[public] Class
	{
		var[public] int Integer;
		method entrypoint void main()
		{
			var int thingy = Multiply(5,3);
			if(thingy > 3)
			{
				return;
			}
		}
		method int Multiply(var int numOne, var int numTwo)
		{
			return numOne * numTwo;
		}		
	}
}