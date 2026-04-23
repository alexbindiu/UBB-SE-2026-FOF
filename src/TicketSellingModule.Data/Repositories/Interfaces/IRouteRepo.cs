using System;
using System.Collections.Generic;
using System.Text;

namespace TicketSellingModule.Data.Repositories.Interfaces;

public interface IRouteRepo
{
    List<Route> GetAllRoutes();
    int AddRoute(Route newRoute);
    void DeleteRoute(int id);
    void UpdateRoute(Route updatedRoute);
    Route GetRouteById(int id);
}
