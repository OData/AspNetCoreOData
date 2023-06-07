using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryBuilder.Query.Validator
{
    public class QueryValidators
    {

        public QueryValidators() { }

        IComputeQueryValidator ComputeQueryValidator { get; set; }

        ICountQueryValidator CountQueryValidator { get; set; }

        IFilterQueryValidator FilterQueryValidator { get; set; }

        IODataQueryValidator ODataQueryValidator { get; set; }

        // QUESTION: Should these types be of interfaces or specific implementations, since this has to be an implementation
        OrderByModelLimitationsValidator OrderByModelLimitationsValidator { get; set; }

        IOrderByQueryValidator OrderByQueryValidator { get; set; }

        ISelectExpandQueryValidator SelectExpandQueryValidator { get; set; }

        ISkipQueryValidator SkipQueryValidator { get; set; }

        ISkipTokenQueryValidator SkipTokenQueryValidator { get; set; }

        ITopQueryValidator TopQueryValidator { get; set; }

        public IComputeQueryValidator GetComputeQueryValidator()
        {
            return ComputeQueryValidator ??= new ComputeQueryValidator();
        }

        public ICountQueryValidator GetCountQueryValidator()
        {
            return CountQueryValidator ??= new CountQueryValidator();
        }

        public IFilterQueryValidator GetFilterQueryValidator()
        {
            return FilterQueryValidator ??= new FilterQueryValidator();
        }

        public IODataQueryValidator GetODataQueryValidator()
        {
            return ODataQueryValidator ??= new ODataQueryValidator();
        }


        // QUESTION: This constructor requires a boolean parameter, so does it still make sense to have a default instantiation?
        //public OrderByModelLimitationsValidator GetOrderByModelLimitationsValidator()
        //{
        //    return OrderByModelLimitationsValidator ??= new OrderByModelLimitationsValidator();
        //}

        public IOrderByQueryValidator GetOrderByQueryValidator()
        {
            return OrderByQueryValidator ??= new OrderByQueryValidator();
        }

        public ISelectExpandQueryValidator GetSelectExpandQueryValidator()
        {
            return SelectExpandQueryValidator ??= new SelectExpandQueryValidator();
        }

        public ISkipQueryValidator GetSkipQueryValidator()
        {
            return SkipQueryValidator ??= new SkipQueryValidator();
        }

        public ISkipTokenQueryValidator GetSkipTokenQueryValidator()
        {
            return SkipTokenQueryValidator ??= new SkipTokenQueryValidator();
        }

        public ITopQueryValidator GetTopQueryValidator()
        {
            return TopQueryValidator ??= new TopQueryValidator();
        }

        /*public void SetComputeQueryValidator(IComputeQueryValidator computeQueryValidator)
        {
            ComputeQueryValidator = computeQueryValidator;
        }*/
    }
}
