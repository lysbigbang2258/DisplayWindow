// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   FFT变换
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ArrayDisplay.Net {
    using System;

    /// <summary>
    ///     The ff t_ trans form.
    ///     FFT变换
    /// </summary>
    public class FFT_TransForm {
        /// <summary>
        ///     FFT输入数据重排
        /// </summary>
        /// <param name="array">输入数据</param>
        /// <returns>输出数据</returns>
        public float[] MySort(float[] array) {
            int len = array.Length;
            var result = new float[len];
            array.CopyTo(result, 0);
            int cur = 0;

            for(int i = 1; i < len; i++) {
                int k = len >> 1;
                while(cur >= k) {
                    cur = cur - k;
                    k = k >> 1;
                }
                cur = cur + k;
                if (i < cur) {
                    float tmep = result[i];
                    result[i] = result[cur];
                    result[cur] = tmep;
                }
            }

            return result;
        }

        /// <summary>
        ///     进行傅里叶变换
        /// </summary>
        /// <param name="source">输入数据（长度为2的幂整数倍）</param>
        /// <returns>输出数据 </returns>
        public float[] FFT(float[] source) {
            var com = this.Dit2_FFT(source);
            var result = new float[source.Length];
            for(int i = 0; i < source.Length; i++) {
                result[i] = com[i].Modulus();
                double temp = result[i];
                temp = 20 * Math.Log10(temp / source.Length * 2); // 转化为db显示
                result[i] = (float)temp;
            }
            return result;
        }

        /// <summary>
        ///     非迭代法傅里叶变换
        /// </summary>
        /// <param name="srcComplexs">原始数据</param>
        /// <returns>变换后数据</returns>
        public Complex[] Dit2_FFT(float[] srcComplexs) {
            int len = srcComplexs.Length;
            var resigndata = new float[len];
            resigndata = this.MySort(srcComplexs);
            var com = new Complex[len];
            for(int i = 0; i < len; i++) {
                com[i] = new Complex(resigndata[i]);
            }
            float wn_r, wn_i; // 旋转因子
            int logNum = (int)(Math.Log(len) / Math.Log(2)); // 蝶形图级数
            for(int l = 0; l < logNum; l++) {
                int space = (int)Math.Pow(2, l);
                int num = space; // 旋转因子个数
                float temp1_r, temp1_i, temp2_r, temp2_i;
                int p = (int)Math.Pow(2, logNum - 1 - l); // 同一旋转因子有p个蝶
                for(int i = 0; i < num; i++) {
                    wn_r = (float)Math.Cos(2 * Math.PI / len * p * i);
                    wn_i = (float)-Math.Sin(2 * Math.PI / len * p * i);
                    for(int j = 0, n = i; j < p; j++, n += (int)Math.Pow(2, l + 1)) {
                        temp1_r = com[n].Re;
                        temp1_i = com[n].Im;
                        temp2_r = com[n + space].Re;
                        temp2_i = com[n + space].Im; // 为蝶形的两个输入数据作副本，对副本进行计算，避免数据被修改后参加下一次计算
                        com[n].Re = temp1_r + (temp2_r * wn_r) - (temp2_i * wn_i);
                        com[n].Im = temp1_i + (temp2_i * wn_r) + (temp2_r * wn_i);
                        com[n + space].Re = temp1_r - (temp2_r * wn_r) + (temp2_i * wn_i);
                        com[n + space].Im = temp1_i - (temp2_i * wn_r) - (temp2_r * wn_i);
                    }
                }
            }

            return com;
        }
    }
}
