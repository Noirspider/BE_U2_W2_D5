using BE_U2_W2_D5.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace BE_U2_W2_D5.Controllers
{
    public class CostiController : Controller
    {
        [Authorize]
        public ActionResult ElencoClientiConTotale()
        {
            List<Clienti> clienti = new List<Clienti>();
            Dictionary<int, decimal> totalePerCliente = new Dictionary<int, decimal>();

            using (SqlConnection con = Connessioni.GetConnection())
            {
                con.Open();

                // Recupera tutti i clienti
                string queryClienti = "SELECT IdCliente, Nome, Cognome FROM Cliente";
                using (SqlCommand cmd = new SqlCommand(queryClienti, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idCliente = reader.IsDBNull(reader.GetOrdinal("IdCliente")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdCliente"));
                            string nome = reader.IsDBNull(reader.GetOrdinal("Nome")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nome"));
                            string cognome = reader.IsDBNull(reader.GetOrdinal("Cognome")) ? string.Empty : reader.GetString(reader.GetOrdinal("Cognome"));

                            Clienti cliente = new Clienti
                            {
                                IdCliente = idCliente,
                                Nome = nome,
                                Cognome = cognome
                            };

                            clienti.Add(cliente);
                            // Inizializza il totale a 0 per ogni cliente
                            totalePerCliente[idCliente] = 0;
                        }
                    }
                }

                // Calcola il totale per ogni cliente sommando tariffe di soggiorno e servizi
                string queryTotali = @"
    SELECT c.IdCliente, p.IdPrenotazione,
           SUM(ISNULL(p.TariffaSoggiorno, 0)) + SUM(ISNULL(s.PrezzoTot, 0)) AS Totale
    FROM Cliente c
    LEFT JOIN Prenotazione p ON c.IdCliente = p.IdCliente
    LEFT JOIN Servizio s ON p.IdPrenotazione = s.IdPrenotazione
    WHERE c.IdCliente IS NOT NULL
    GROUP BY c.IdCliente, p.IdPrenotazione";


                using (SqlCommand cmd = new SqlCommand(queryTotali, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int idCliente = reader.IsDBNull(reader.GetOrdinal("IdCliente")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdCliente"));
                            int idPrenotazione = reader.IsDBNull(reader.GetOrdinal("IdPrenotazione")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdPrenotazione"));
                            decimal totale = reader.IsDBNull(reader.GetOrdinal("Totale")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Totale"));

                            if (totalePerCliente.ContainsKey(idCliente))
                            {
                                totalePerCliente[idCliente] = totale;
                            }
                        }
                    }
                }
            }

            ViewBag.TotalePerCliente = totalePerCliente;
            return View(clienti);
        }
    }
}
