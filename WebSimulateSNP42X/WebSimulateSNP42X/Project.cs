using GenHTTP.Api.Content;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSimulateSNP42X.Services;

namespace WebSimulateSNP42X
{
    public static class Project
    {
        public static IHandlerBuilder Setup()
        {


            return Layout.Create()
                         .AddService<BookService>("books")
                         .AddService<ParkingLotService>("parkingLot")
                         .Add(CorsPolicy.Permissive());
        }

    }

}
