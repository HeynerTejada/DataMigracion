using AppDesktop.ViewModel.Model;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppDesktop.ViewModel
{
    public interface IMainViewModel
    {
        int CantidadRegistros { get; set; }
        int CantidadRegistrosProcesados { get; set; }
        ICommand CmdCrearArchivos { get; }
        ICommand CmdGenerarArchivos { get; }
        ICommand CmdMigrarDatos { get; }
        ICommand CmdSeleccionarArchivo { get; }
        bool isPadronDetalle { get; set; }
        ICollection<ModelArchivos> ListadoArchivos { get; set; }
        string textoArchivo { get; set; }
        string TiempoEjecucion { get; set; }

    }
}