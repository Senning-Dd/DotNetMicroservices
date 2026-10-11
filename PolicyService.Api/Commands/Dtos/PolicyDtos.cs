namespace PolicyService.Api.Commands.Dtos;

public enum QuestionType
{
    Numeric,
    Choice
}

public class QuestionAnswer
{
    public string QuestionCode { get; set; }
    public QuestionType QuestionType { get; set; }
}

public class ChoiceQuestionAnswer : QuestionAnswer
{
    public string Answer { get; set; }
}

public class NumericQuestionAnswer : QuestionAnswer
{
    public decimal Answer { get; set; }
}

public class PersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxId { get; set; }
}

public class AddressDto
{
    public string Country { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
}

