using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPF_Memory
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Lista de palabras
        // Cada palabra se corresponde con un botón
        private String[] palabras = new String[] { "Pájaro", "Pájaro", "Perro",
        "Perro", "Gato", "Gato", "Tortuga", "Tortuga", "Ratón", "Ratón", 
        "Ardilla", "Ardilla", "Serpiente", "Serpiente", "Iguana", "Iguana"};

        // Parámetros del juego
        // Botones visibles, eliminados y clicks realizados por el usuario
        private int numBotonesVisibles = 0;
        private int numBotonesEliminados = 0;
        private List<int> botonesVisibles = new List<int> {};
        private int clicks = 0;

        //  Bloqueo de los botones
        bool bloqueoBotones = false;

        private int ticksReloj = 60;
        private DispatcherTimer temporizador;

        public MainWindow()
        {
            // Se inicializan los componentes y el reloj
            InitializeComponent();

            // Empieza el juego
            inicializarJuego();
        }

        // Ejecuta los ticks del reloj del juego
        private void timerTick(object sender, EventArgs e)
        {
            //DispatcherTimer timer = (DispatcherTimer)sender;
            contadorTiempo.Text = ticksReloj.ToString();
            if (ticksReloj-- == 0)
            {
                // Finaliza el juego
                finalizarJuego();
            }
        }

        // Detiene el reloj del juego
        private void pararReloj ()
        {
            temporizador.Stop();
        }

        // Inicializa el juego
        private void inicializarJuego ()
        {
            // Se configura el reloj del juego
            temporizador = new DispatcherTimer();
            temporizador.Interval = new TimeSpan(0, 0, 1); // Se ejecutará un tick cada segundo
            temporizador.Tick += new EventHandler(timerTick);
            ticksReloj = 60;
            contadorTiempo.Text = ticksReloj.ToString();
            temporizador.Start();

            // Se mezclan las palabras
            Random rnd = new Random();
            palabras = palabras.OrderBy(x => rnd.Next()).ToArray();
            //palabras.ToList().ForEach(i => Console.WriteLine(i.ToString()));

            // Por defecto, no se ve ninguna palabra en los botones
            numBotonesVisibles = 0;

            // Al iniciar el juego, no se ha eliminado ningún botón
            numBotonesEliminados = 0;

            // Se inicializa la puntuación
            clicks = 0;
            contadorClicks.Text = clicks.ToString();

            // Se borra el contenido de todos los botones
            borrarBotones();

            // Se muestran los botones (se hacen visibles)
            mostrarBotones();
        }

        // Finaliza el juego y muestra la puntuación al usuario
        private void finalizarJuego()
        {
            MessageBoxResult respuesta = MessageBoxResult.No;
            String mensaje = "";

            // Se detiene el reloj del juego
            pararReloj();

            // Pueden suceder dos eventos, o bien que se acabe el tiempo
            // o bien que se eliminen todos los botones antes de que el
            // tiempo acabe o justo cuando el tiempo ha acabado.
            if (numBotonesEliminados != 16)
            {
                // No se lograron eliminar todos los botones
                mensaje = "¡Fin del juego!\n\nUsaste " + clicks + " clicks\n\n" +
                    "¿Jugar de de nuevo?";
            }
            else
            {
                // Se lograron eliminar todos los botones
                mensaje = "¡Acertaste las parejas a tiempo!\n\nUsaste " +
                    clicks + " clicks\n\n ¿Jugar de nuevo?";
            }

            // Se muestra el mensaje al usuario
            respuesta = MessageBox.Show(mensaje, "WPF_Memory", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Según la pulsación del usuario, se comienza un nuevo juego
            if (respuesta == MessageBoxResult.Yes)
                inicializarJuego();
            else
                this.Close();
        }

        // Borra el contenido de los botones
        private void borrarBotones()
        {
            // Se borra el contenido de todos los botones
            for (int i = 0; i < 16; i++)
            {
                Button boton = (Button) this.FindName("boton" + i.ToString());
                boton.Content = "";
            }

            // Se limpia la lista de ids de botones visibles
            botonesVisibles.Clear();
            numBotonesVisibles = 0;
        }

        // Muestra todos los botones en la pantalla
        private void mostrarBotones()
        {
            for (int i = 0; i < 16; i++)
            {
                Button boton = (Button)this.FindName("boton" + i.ToString());
                boton.Visibility = Visibility.Visible;
            }

            // Se limpia la lista de ids de botones visibles
            botonesVisibles.Clear();
        }

        // Bloquea los botones para que el usuario no pueda pulsarlos
        private void bloquearBotones()
        {
            bloqueoBotones = true;
        }

        // Desbloquea los botones para que el usuario pueda pulsarlos
        private void desbloquearBotones()
        {
            bloqueoBotones = false;
        }

        // Valida los botones pulsados para comprobar si son iguales
        // El método es asincrónico para no parar el hilo de la UI
        // al hacer un delay (para que el usuario vea los botones pulsados)
        private async void validarBotones()
        {
            // Se obtienen los dos botones pulsados
            int idBoton1 = botonesVisibles.ElementAt(0);
            botonesVisibles.RemoveAt(0);
            Button boton1 = (Button)this.FindName("boton" + idBoton1);

            int idBoton2 = botonesVisibles.ElementAt(0);
            botonesVisibles.RemoveAt(0);
            Button boton2 = (Button)this.FindName("boton" + idBoton2);

            // Si el contenido de los botones es igual se suman los puntos
            // y se ocultan los botones
            if (boton1.Content == boton2.Content)
            {
                // Se bloquean los botones para que no puedan ser pulsados
                bloquearBotones();

                // Esperamos 0,75 segundos
                await Task.Delay(750);

                // Se ocultan los dos botones de la pantalla
                boton1.Visibility = Visibility.Hidden;
                boton2.Visibility = Visibility.Hidden;
                numBotonesEliminados += 2;

                // Se borra el contenido de los botones
                borrarBotones();

                // Se desbloquean los botones para que puedan ser pulsados
                desbloquearBotones();

                // Se comprueba si todos los botones han sido borrados
                if (numBotonesEliminados == 16)
                    finalizarJuego();
            }
            else
            {
                // Se bloquean los botones para que no puedan ser pulsados
                bloquearBotones();
                
                // Esperamos 0,75 segundos
                await Task.Delay(750);

                // Los botones no son iguales
                // Se borra el contenido de los botones
                borrarBotones();

                // Se desbloquean los botones para que puedan ser pulsados
                desbloquearBotones();
            }
        }

        // Actualiza el contenido de cada uno de los botones de la 
        // ventana, para asegurar que se muestra el contenido
        private void actualizarContenidoBotones()
        {
            for (int i = 0; i < 16; i++)
            {
                Button boton = (Button)this.FindName("boton" + i.ToString());
                boton.UpdateLayout();
            }
        }

        // Contabiliza los clicks realizados por el usuario y actualiza
        // el valor en la interfaz
        private void actualizarClicksUsuario()
        {
            clicks += 1;
            contadorClicks.Text = clicks.ToString();
        }

        // Manejador de eventos de los botones
        private void buttonClick(object sender, RoutedEventArgs e)
        {
            // Se ejecuta el evento siempre que los botones no estén
            // bloqueados (tras haber pulsado dos)
            if (!bloqueoBotones)
            {
                // Se obtiene el botón que se ha pulsado (su contenido
                // está oculto por defecto)
                Button boton = (Button)sender;
                int indiceBoton = int.Parse(boton.Name.Substring(5));
                boton = (Button)this.FindName("boton" + indiceBoton);
                boton.Content = palabras[indiceBoton];

                // Se contabiliza el número de botones pulsados
                if (!botonesVisibles.Contains(indiceBoton))
                {
                    botonesVisibles.Add(indiceBoton);
                    numBotonesVisibles++;

                    // Se contabilizan los clicks realizados por el usuario
                    actualizarClicksUsuario();

                    // Si hay dos botones visibles, se valida su contenido
                    if (numBotonesVisibles == 2)
                    {
                        // Hacemos que el hilo principal se duerma para que
                        // el usuario pueda revisar si su respuesta es correcta o no
                        actualizarContenidoBotones();
                        validarBotones();
                    }
                }
            }
        }
    }
}
