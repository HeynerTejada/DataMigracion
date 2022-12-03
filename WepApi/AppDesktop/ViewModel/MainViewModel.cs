using AppClases;
using AppDesktop.Base;
using AppDesktop.Utils;
using AppDesktop.ViewModel.Model;
using Bogus;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppDesktop.ViewModel
{
    public class MainViewModel : Mvvm, IMainViewModel
    {
        private int CantidadLineas = 100000;
        private int CantidadLineasRecorrido = 0;
        private string CaracterSeparador = string.Empty;
        private TaskScheduler scheduler;

        private ICollection<ModelArchivos> _listadoArchivos;

        public ICollection<ModelArchivos> ListadoArchivos
        {
            get => _listadoArchivos;
            set => SetProperty(ref _listadoArchivos, value);
        }

        private string _tiempoEjecucion;

        public string TiempoEjecucion
        {
            get => _tiempoEjecucion;
            set => SetProperty(ref _tiempoEjecucion, value);
        }

        private int _cantidadRegistros;

        public int CantidadRegistros
        {
            get => _cantidadRegistros;
            set => SetProperty(ref _cantidadRegistros, value);
        }

        private int _cantidadRegistrosProcesados;

        public int CantidadRegistrosProcesados
        {
            get => _cantidadRegistrosProcesados;
            set => SetProperty(ref _cantidadRegistrosProcesados, value);
        }

        private bool _isPadronDetalle;

        public bool isPadronDetalle
        {
            get => _isPadronDetalle;
            set
            {
                CaracterSeparador = value ? "|" : ",";
                SetProperty(ref _isPadronDetalle, value);
            }
        }

        private string _textoArchivo = string.Empty;

        public string textoArchivo
        {
            get => _textoArchivo;
            set => SetProperty(ref _textoArchivo, value);
        }

        private ICommand _cmdSeleccionarArchivo;
        private ICommand _cmdGenerarArchivos;
        private ICommand _cmdMigrarDatos;
        private ICommand _cmdCrearArchivos;

        public ICommand CmdSeleccionarArchivo => _cmdSeleccionarArchivo ??= new DelegateCommand(SelectedFile);
        public ICommand CmdMigrarDatos => _cmdMigrarDatos ??= new DelegateCommand(ExecuteExportData, CanExecuteExportData);
        public ICommand CmdGenerarArchivos => _cmdGenerarArchivos ??= new DelegateCommand(GenerarArchivo);
        public ICommand CmdCrearArchivos => _cmdCrearArchivos ??= new DelegateCommand(ExecuteCreateFiles, CanExecuteCreateFiles);

        public MainViewModel()
        {
            isPadronDetalle=true;
            VerificarArchivos();
        }

        private void SelectedFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Txt files (*.txt)|*.txt|CSV files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
                textoArchivo = openFileDialog.FileName;
        }

        private bool CanExecuteExportData()
        {
            if (ListadoArchivos==null) return false;
            if (ListadoArchivos.Count==0) return false;

            return true;
        }

        private void ExecuteExportData()
        {
            Stopwatch timeMeasure = new Stopwatch();
            timeMeasure.Start();
            MigrarInformacion();
            timeMeasure.Stop();

            TimeSpan ts = timeMeasure.Elapsed;
            TiempoEjecucion = string.Format("Elapsed Time is {0:00}:{1:00}:{2:00}.{3}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        private string ReemplazarTexto(string newData)
        {
            return newData.Replace(@"\xEF\xBF\xBD", "?");
        }

        private void ExecuteExportPadronDetalleSingle(string[] fila, ref DataMigacion data)
        {
            data = new DataMigacion()
            {
                Cui=fila[0],
                DatosCompletos= ReemplazarTexto(fila[1])
            };
        }

        private void ExecuteExportPadronDetalleList(string[] fila, ref ICollection<DataMigacion> Lista)
        {
            DataMigacion data = new DataMigacion()
            {
                Cui=fila[0],
                DatosCompletos=fila[1],
            };
            Lista.Add(data);
        }

        private void ExecuteExportStream(string rutaArchivo)
        {
            int counter = 0;

            if (string.IsNullOrEmpty(textoArchivo)) return;

            using (StreamReader leer = new StreamReader(textoArchivo))
            {
                ICollection<DataMigacion> Lista = new List<DataMigacion>();
                while (!leer.EndOfStream)
                {
                    string line = leer.ReadLine();
                    if (counter>=1)
                    {
                        string[] fila = line.Split(CaracterSeparador);
                        ExecuteExportPadronDetalleList(fila, ref Lista);
                    }
                    counter++;
                }
                string result = LogicaHttpClient.PostSync(Rutas.urlDataMigracionAsync, Lista);
            }
        }

        //Archivo Completo
        private void ExecuteExportFile(string rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo)) return;

            int counter = 0;
            ICollection<DataMigacion> ListaMigracion = new List<DataMigacion>();

            foreach (string line in File.ReadLines(textoArchivo, Encoding.UTF8))
            {
                if (counter>=1)
                {
                    if (isPadronDetalle)
                    {
                        string[] fila = line.Split(CaracterSeparador);

                        ExecuteExportPadronDetalleList(fila, ref ListaMigracion);

                        if (ListaMigracion.Count>=CantidadLineas)
                        {
                            LogicaHttpClient.PostSync(Rutas.urlDataMigracion, ListaMigracion);
                            ListaMigracion.Clear();
                        }
                    }
                }

                counter++;
            }
        }

        private async Task ExecuteExportFileV1(string rutaArchivo, int _cantidadLineas)
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(rutaArchivo)) return;

                int counter = 0;
                ICollection<DataMigacion> ListaMigracion = new List<DataMigacion>();

                CantidadRegistros = File.ReadLines(rutaArchivo).Count();

                foreach (string line in File.ReadLines(rutaArchivo, Encoding.UTF8))
                {
                    if (counter>=1)
                    {
                        string[] fila = line.Split(CaracterSeparador);

                        ExecuteExportPadronDetalleList(fila, ref ListaMigracion);

                        if (ListaMigracion.Count>=_cantidadLineas)
                        {
                            LogicaHttpClient.PostSync(Rutas.urlDataMigracion, ListaMigracion);
                            ListaMigracion.Clear();
                        }
                    }
                    counter++;
                    CantidadRegistrosProcesados= counter;
                }
            });
        }

        private void ExecuteExportFileV2(ModelArchivos item)
        {
            if (string.IsNullOrEmpty(item.RutaArchivo)) return;

            int counter = 0;
            ICollection<DataMigacion> ListaMigracion = new List<DataMigacion>();

            CantidadRegistros = File.ReadLines(item.RutaArchivo).Count();

            foreach (string line in File.ReadLines(item.RutaArchivo, Encoding.UTF8))
            {
                string[] fila = line.Split(CaracterSeparador);

                ExecuteExportPadronDetalleList(fila, ref ListaMigracion);

                counter++;

                item.CantidadLineasProcesadas=counter;
            }
            LogicaHttpClient.PostSync(Rutas.urlDataMigracion, ListaMigracion);
        }

        private void ExecuteExportFileV3(ModelArchivos item)
        {
            if (string.IsNullOrEmpty(item.RutaArchivo)) return;

            int counter = 0;
            DataMigacion ListaMigracion = null;

            CantidadRegistros = File.ReadLines(item.RutaArchivo).Count();

            Parallel.ForEach(File.ReadLines(item.RutaArchivo, Encoding.UTF8), line =>
            {
                string[] fila = line.Split(CaracterSeparador);

                ExecuteExportPadronDetalleSingle(fila, ref ListaMigracion);

                LogicaHttpClient.PostSync(Rutas.urlDataMigracion, ListaMigracion);

                counter++;

                item.CantidadLineasProcesadas=counter;
            });
        }

        private void GenerarArchivo()
        {
            var testData = new Faker<DataMigacion>()
            .StrictMode(true)
            .RuleFor(o => o.Cui, f => String.Format("{0:d11}", (DateTime.Now.Ticks / 10) % 1000000000))
            .RuleFor(o => o.DatosCompletos, f => f.Person.FullName).Generate(1000000);

            if (!Directory.Exists(Rutas.rutaPrincipal))
                System.IO.Directory.CreateDirectory(Rutas.rutaPrincipal);

            using (StreamWriter file = new StreamWriter(Rutas.rutaPrincipal+ @"/EscribeLineas.txt"))
            {
                foreach (var item in testData)
                {
                    file.WriteLine(string.Concat(item.Cui, CaracterSeparador, item.DatosCompletos));
                }
            }
        }

        private bool CanExecuteCreateFiles()
        {
            return !string.IsNullOrEmpty(textoArchivo);
        }

        private async void ExecuteCreateFiles()
        {
            try
            {
                await Task.Run(() =>
                {
                    CantidadRegistros = File.ReadLines(textoArchivo).Count();
                    StreamWriter sw = null;
                    int TotalPaginas = CantidadRegistros / CantidadLineas;
                    int pagina = 1;
                    int recorrido = 1;
                    string filename = string.Empty;

                    if (!Directory.Exists(Rutas.rutaParticion))
                        System.IO.Directory.CreateDirectory(Rutas.rutaParticion);

                    //Recorrer el archivo
                    foreach (string line in File.ReadLines(textoArchivo, Encoding.Default))
                    {
                        if (recorrido==1)
                        {
                            filename = Rutas.rutaParticion+ "/file" + pagina.ToString() + ".txt";
                            sw = new StreamWriter(filename);
                        }

                        if (line.Contains(CaracterSeparador))
                        {
                            sw.WriteLine(line);
                        }

                        if (recorrido==CantidadLineas)
                        {
                            sw.Close();
                            recorrido=1;
                            pagina++;
                        }
                        else
                        {
                            if (CantidadLineasRecorrido+1==CantidadRegistros)
                            {
                                sw.Close();
                            }
                            else
                            {
                                recorrido++;
                            }
                        }

                        CantidadLineasRecorrido++;

                        CantidadRegistrosProcesados = recorrido;
                    }

                    VerificarArchivos();
                });
            }
            catch (Exception error)
            {
                var mensaje = error.Message;
            }
        }

        private void VerificarArchivos()
        {
            try
            {
                string[] files = Directory.GetFiles(Rutas.rutaParticion); // Obtener archivos

                ListadoArchivos= files
                    .Select(d => new ModelArchivos
                    {
                        RutaArchivo= d,
                        CantidadLineas = File.ReadLines(d).Count()
                    }).ToList();
            }
            catch (Exception error)
            {
                var message = string.Empty;
            }
        }

        private void MigrarInformacion()
        {
            //Se hace uno por uno pero demora
            //foreach (ModelArchivos archivo in ListadoArchivos)
            //{
            //    ExecuteExportFileV2(archivo);
            //}

            //Se hace indistito a la cantidad de hilos permitidos
            Parallel.ForEach(ListadoArchivos, archivo =>
            {
                ExecuteExportFileV2(archivo);
            });

            //List<Task> hilos = new List<Task>();

            //await Task.Run(() =>
            //{
            //    foreach (ModelArchivos archivo in ListadoArchivos)
            //    {
            //        int CantidadRegistros = File.ReadLines(archivo.RutaArchivo).Count();

            //        archivo.CantidadLineas= CantidadRegistros;

            //        hilos.Add(ExecuteExportFileV2(archivo));
            //    }
            //});

            //await Task.WhenAll(hilos);
            //Parallel.ForEach(ListadoArchivos, archivo =>
            //{
            //    ExecuteExportFileV1(archivo);
            //    //Console.WriteLine("Fruit Name: {0}, Thread Id= {1}", fruit, Thread.CurrentThread.ManagedThreadId);
            //});
        }
    }
}