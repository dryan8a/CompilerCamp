//Bubble sort

namespace Sorts
{
    class[public] static BubbleSort
    {
        method[public] static int[] Sort(int[] nums)
        {
            for(var int i = 0;i < nums.Length - 1;i++)
            {
                for(var int j = 0;j < nums.Length - i - 1; j++)
                {
                    if(nums[j] > nums[j+1])
                    {
                        var int temp = nums[j];
                        nums[j] = nums[j+1];
                        nums[j+1] = temp;
                    }
                }
            }
            return nums;
        }
    }

    class[public] Program
    {
        method[pubilc] entrypoint void Main()
        {
            var int[] nums = {1,7,(-)1,3*2,2};
            //Sorts the numbers in nums using BubbleSort
            nums = BubbleSort.Sort(nums);
        }
    }
}