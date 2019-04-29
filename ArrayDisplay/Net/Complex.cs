// --------------------------------------------------------------------------------------------------------------------  
// <summary>
//   The complex.
//   表示复数
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ArrayDisplay.Net {
    using System;

    /// <summary>
    /// The complex.
    ///  表示复数
    /// </summary>
    public class Complex {
        /// <summary>
        /// Initializes a new instance of the <see cref="Complex"/> class.
        /// </summary>
        /// <param name="r">
        /// 实部
        /// </param>
        /// <param name="im">
        /// 虚部
        /// </param>
        public Complex(float r = 0, float im = 0) {
            this.Re = r;
            this.Im = im;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Complex"/> class.
        /// </summary>
        /// <param name="cp">
        /// The cp.
        /// </param>
        public Complex(Complex cp) {
            this.Re = cp.Re;
            this.Im = cp.Im;
        }

        #region 属性

        /// <summary>
        /// Gets or sets the re.
        /// </summary>
        public float Re
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the im.
        /// </summary>
        public float Im
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// The modulus.
        /// </summary>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public float Modulus() {
            return (float)Math.Sqrt((this.Re * this.Re) + (this.Im * this.Im));
        }

        /// <summary>
        /// The +.
        /// </summary>
        /// <param name="c1">
        /// The c 1.
        /// </param>
        /// <param name="c2">
        /// The c 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator +(Complex c1, Complex c2) {
            return new Complex(c1.Re + c2.Re, c1.Im + c2.Im);
        }

        /// <summary>
        /// The -.
        /// </summary>
        /// <param name="c1">
        /// The c 1.
        /// </param>
        /// <param name="c2">
        /// The c 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator -(Complex c1, Complex c2) {
            return new Complex(c1.Re - c2.Re, c1.Im - c2.Im);
        }

        /// <summary>
        /// The -.
        /// </summary>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator -(float d, Complex c) {
            return new Complex(d - c.Re, -c.Im);
        }

        /// <summary>
        /// The *.
        /// </summary>
        /// <param name="c1">
        /// The c 1.
        /// </param>
        /// <param name="c2">
        /// The c 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator *(Complex c1, Complex c2) {
            return new Complex((c1.Re * c2.Re) - (c1.Im * c2.Im), (c1.Re * c2.Im) + (c2.Re * c1.Im));
        }

        /// <summary>
        /// The *.
        /// </summary>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator *(float d, Complex c) {
            return new Complex(c.Re * d, c.Im * d);
        }

        /// <summary>
        /// The *.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator *(Complex c, float d) {
            return new Complex(c.Re * d, c.Im * d);
        }

        /// <summary>
        /// The /.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator /(Complex c, float d) {
            return new Complex(c.Re / d, c.Im / d);
        }

        /// <summary>
        /// The /.
        /// </summary>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator /(float d, Complex c) {
            float temp = d / ((c.Re * c.Re) + (c.Im * c.Im));
            return new Complex(c.Re * temp, -c.Im * temp);
        }

        /// <summary>
        /// The /.
        /// </summary>
        /// <param name="c1">
        /// The c 1.
        /// </param>
        /// <param name="c2">
        /// The c 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static Complex operator /(Complex c1, Complex c2) {
            float temp = 1 / ((c2.Re * c2.Re) + (c2.Im * c2.Im));
            return new Complex(((c1.Re * c2.Re) - (c1.Im * c2.Im)) * temp, ((-c1.Re * c2.Im) + (c2.Re * c1.Im)) * temp);
        }

        #region Overrides of Object

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString() {
            string retStr;
            if (Math.Abs(this.Im) < 0.0001) {
                retStr = this.Re.ToString("f4");
            }
            else if (Math.Abs(this.Re) < 0.0001) {
                if (this.Im > 0) {
                    retStr = "j" + this.Im.ToString("f4");
                }
                else {
                    retStr = "-j" + (0 - this.Im).ToString("f4");
                }
            }
            else {
                if (this.Im > 0) {
                    retStr = this.Re.ToString("f4") + "+j" + this.Im.ToString("f4");
                }
                else {
                    retStr = this.Re.ToString("f4") + "-j" + (0 - this.Im).ToString("f4");
                }
            }

            retStr += " ";
            return retStr;
        }

        #endregion
    }
}