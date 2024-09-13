using DistributedIntegration.Common;

namespace DistributedIntegration.IntegrationTask
{
    public class IntegrateTask : ITask
    {
        public object Execute(Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("a", out object aObj) || !TryConvertToDouble(aObj, out double a))
                throw new ArgumentException("Parameter 'a' is missing or cannot be converted to double");

            if (!parameters.TryGetValue("b", out object bObj) || !TryConvertToDouble(bObj, out double b))
                throw new ArgumentException("Parameter 'b' is missing or cannot be converted to double");

            if (!parameters.TryGetValue("n", out object nObj) || !TryConvertToInt(nObj, out int n))
                throw new ArgumentException("Parameter 'n' is missing or cannot be converted to int");

            return Integrate(a, b, n);
        }

        private bool TryConvertToDouble(object obj, out double result)
        {
            if (obj is double d)
            {
                result = d;
                return true;
            }
            if (obj is float f)
            {
                result = f;
                return true;
            }
            if (obj is int i)
            {
                result = i;
                return true;
            }
            if (obj is long l)
            {
                result = l;
                return true;
            }
            return double.TryParse(obj.ToString(), out result);
        }

        private bool TryConvertToInt(object obj, out int result)
        {
            if (obj is int i)
            {
                result = i;
                return true;
            }
            if (obj is long l)
            {
                if (l >= int.MinValue && l <= int.MaxValue)
                {
                    result = (int)l;
                    return true;
                }
            }
            return int.TryParse(obj.ToString(), out result);
        }

        private double Integrate(double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0.5 * (Function(a) + Function(b));

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += Function(x);
            }

            return sum * h;
        }

        private double Function(double x)
        {
            return Math.Sin(x);
        }
    }
}
