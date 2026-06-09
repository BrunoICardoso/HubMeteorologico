using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Interface;
using HubMeteorologico.Infrastructure.Repository.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubMeteorologico.Infrastructure.Repository;

public class MapaFazendaLavouraRepository : Repository<MapaFazendaLavoura>, IMapaFazendaLavouraRepository
{
    public MapaFazendaLavouraRepository(IDbSession session) : base(session) { }
}
