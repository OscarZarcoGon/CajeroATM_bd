namespace Cajero.ATM
{
    public class Cliente
    {
        public int ID { get; set; }
        public int NumeroCliente { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public long Telefono { get; set; }

        public Cliente() { }

        public Cliente(int numeroCliente, string nombre, DateTime fechaNacimiento, long telefono)
        {
            this.NumeroCliente = numeroCliente;
            this.Nombre = nombre;
            this.FechaNacimiento = fechaNacimiento;
            this.Telefono = telefono;
        }
    }
}
