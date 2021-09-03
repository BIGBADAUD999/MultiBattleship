
namespace ServerData
{
    public class Field
    {
        private char x;
        private char y;
        private char who;
        public string str;

        public Field(string label)
        {
            who = label[0];
            x = label[2];
            y = label[3];
            str = label;
        }
    }
}
