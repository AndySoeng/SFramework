using System.Collections.Generic;


namespace Ex
{
    public static class ExList
    {
        /// <summary>
        /// 从列表中获取一定数量的不重复，注意不要超过列表上限
        /// </summary>
        /// <param name="paramList"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(List<T> paramList, int count)
        {
            if (paramList.Count < count)
            {
                return paramList;
            }

            System.Random random = new System.Random();
            List<int> tempList = new List<int>();
            List<T> newList = new List<T>();
            int temp = 0;
            for (int i = 0; i < count; i++)
            {
                temp = random.Next(paramList.Count); //将产生的随机数作为被抽list的索引
                if (!tempList.Contains(temp))
                {
                    tempList.Add(temp);
                    newList.Add(paramList[temp]);
                }
                else
                {
                    i--;
                }
            }

            return newList;
        }
    }
}