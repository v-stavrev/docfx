using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers;
using Xunit;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.Tests
{
    public class ListParserTests
    {
        [Fact]
        public void CanParseFile1()
        {
            const string file1 = @"# Advanced Section

This section contains information for advanced concepts, systems and deep dive in the security.
It is intended primarily for advanced ERP implementation consultants.

## Advanced Systems

- [Documents](documents/index.md) - document-related concepts and services.
- [Custom Attributes](custom-attributes/overview.md) - user-defined data attributes.
- [Calculated Attributes](calculated-attributes/overview.md) - user-defined calculations.
- [Business Rules](business-rules/overview.md) - user-defined and system business rules.
- [Data Objects](data-objects/index.md) - data object extensibility systems.

## Advanced Concepts

- [Master / Detail Attributes](concepts/master-detail-attributes.md)
- [Object / Relational Mapping](concepts/object-relational-mapping.md)
- [Aggregates](concepts/aggregates.md)

## Temp

[!list folder=""/calculated-attributes"" file=""*"" exclude=""*pro*"" depth=2 limit=50 style=bullet]
";

            var result = ListOperatorParser.Parse(file1);

            Assert.Empty(result.Errors);
            Assert.Single(result.Lists);

            var list = result.Lists[0];

            Assert.Equal("/calculated-attributes", list.FolderPattern);
            Assert.Equal("*", list.FilePattern);
            Assert.Equal("*pro*", list.ExcludePattern);
            Assert.Equal(2, list.Depth);
            Assert.Equal(50, list.Limit);
            Assert.Equal(ListStyle.Bullet, list.Style);
        }

        [Fact]
        public void ParseFileBug3292()
        {
            const string content =
            #region Content
@"---
uid: Crm.Sales.SalesOrders
---
# Crm.Sales.SalesOrders Entity

**Namespace:** [Crm.Sales](Crm.Sales.md)  
**Inherited From:** [General.Documents](General.Documents.md)  

Sales order document headers. Entity: Crm_Sales_Orders

## Default Visualization
Default Display Text Format:  
_{DocumentType.Code}:{DocumentNo} - {DocumentType.TypeName:T}_  
Default Search Member:  
_DocumentNo_  

## Aggregate
An [aggregate](https://docs.erp.net/tech/advanced/concepts/aggregates.html) is a cluster of domain objects that can be treated as a single unit.  

Aggregate Tree  
* [Crm.Sales.SalesOrders](Crm.Sales.SalesOrders.md)  
  * [Crm.Sales.SalesOrderLines](Crm.Sales.SalesOrderLines.md)  
  * [Crm.Sales.SalesOrderPaymentPlans](Crm.Sales.SalesOrderPaymentPlans.md)  
  * [Crm.Sales.SalesOrderPromotionalPackages](Crm.Sales.SalesOrderPromotionalPackages.md)  
  * [General.DocumentAmounts](General.DocumentAmounts.md)  
    * [General.DocumentAmountReferencedDocuments](General.DocumentAmountReferencedDocuments.md)  
  * [General.DocumentComments](General.DocumentComments.md)  
  * [General.DocumentDistributedAmounts](General.DocumentDistributedAmounts.md)  
  * [General.DocumentFileAttachments](General.DocumentFileAttachments.md)  
  * [General.DocumentLineAmounts](General.DocumentLineAmounts.md)  
  * [General.DocumentPrints](General.DocumentPrints.md)  
  * [General.DocumentStateChanges](General.DocumentStateChanges.md)  
  * [General.DocumentVersions](General.DocumentVersions.md)  

## Attributes

| Name | Type | Description |
| ---- | ---- | --- |
| [AdjustmentNumber](Crm.Sales.SalesOrders.md#adjustmentnumber) | int32 | Consecutive number of the correction that this document is applying to the adjusted document. `Required` `Default(0)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [AdjustmentTime](Crm.Sales.SalesOrders.md#adjustmenttime) | datetime __nullable__ | Date/time when the document last has been adjusted by corrective document. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [AdjustmentUser](Crm.Sales.SalesOrders.md#adjustmentuser) | string (64) __nullable__ | The user who adjusted the document. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [CompleteTime](Crm.Sales.SalesOrders.md#completetime) | datetime __nullable__ | Date and time when the document was completed (State set to Completed). `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [CreationTime](Crm.Sales.SalesOrders.md#creationtime) | datetime | Date/Time when the document was created. `Required` `Default(Now)` `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [CreationUser](Crm.Sales.SalesOrders.md#creationuser) | string (64) | The login name of the user, who created the document. `Required` `Filter(like)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [CreditLimitOverride](Crm.Sales.SalesOrders.md#creditlimitoverride) | boolean | Allows the sales order to be released even in the case of violations of credit limit or presence of overdue receivables. `Required` `Default(false)` `Filter(eq)` 
| [CustomerPurchaseOrderDate](Crm.Sales.SalesOrders.md#customerpurchaseorderdate) | date __nullable__ | Issue date of the referent customer purchase order. `Filter(ge;le)` 
| [CustomerPurchaseOrderNo](Crm.Sales.SalesOrders.md#customerpurchaseorderno) | string (20) __nullable__ | Reference number of the customer's purchase order. `Filter(eq;like)` 
| [DeliveryTermsCode](Crm.Sales.SalesOrders.md#deliverytermscode) | [DeliveryTerms](Crm.Sales.SalesOrders.md#deliverytermscode) __nullable__ | Mode of delivery, like CIF, FOB, etc. Used also in Intrastat reporting. 
| [DocumentDate](Crm.Sales.SalesOrders.md#documentdate) | date | The date on which the document was issued. `Required` `Default(Today)` `Filter(eq;ge;le)` `ORD` (Inherited from [Documents](General.Documents.md)) 
| [DocumentNo](Crm.Sales.SalesOrders.md#documentno) | string (20) | Document number, unique within Document_Type_Id. `Required` `Filter(eq;like)` `ORD` (Inherited from [Documents](General.Documents.md)) 
| [DocumentNotes](Crm.Sales.SalesOrders.md#documentnotes) | string (max) __nullable__ | Notes for this Document. (Inherited from [Documents](General.Documents.md)) 
| [DocumentVersion](Crm.Sales.SalesOrders.md#documentversion) | int32 | Consecutive version number, starting with 1. Each update produces a new version of the document. `Required` `Default(1)` `Filter(eq;ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [EntityName](Crm.Sales.SalesOrders.md#entityname) | string (64) | The entity name of the document header. `Required` `Filter(eq)` `ORD` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [FiscalSalesNumber](Crm.Sales.SalesOrders.md#fiscalsalesnumber) | string (32) __nullable__ | Unique number of the sale, assigned for fiscal reporting purposes. The format is according to the applicable legislation. null means that there is no requirement for fiscal sales number for this document or it is unknown. `Filter(multi eq;like)` `ReadOnly` `Introduced in version 19.1` 
| [FromDate](Crm.Sales.SalesOrders.md#fromdate) | date __nullable__ | When selling a service valid only for a period, denotes the beginning of the period. null means that it is unknown or N/A. `Introduced in version 20.1` 
| [Id](Crm.Sales.SalesOrders.md#id) | guid |  
| [IntrastatTransaction<br />NatureCode](Crm.Sales.SalesOrders.md#intrastattransactionnaturecode) | [TransactionNature](Crm.Sales.SalesOrders.md#intrastattransactionnaturecode) __nullable__ | Transaction nature; used for Intrastat reporting. 
| [IntrastatTransportModeCode](Crm.Sales.SalesOrders.md#intrastattransportmodecode) | [TransportMode](Crm.Sales.SalesOrders.md#intrastattransportmodecode) __nullable__ | Transport mode; used for Intrastat reporting. 
| [IsReleased](Crm.Sales.SalesOrders.md#isreleased) | boolean | True if the document is not void and its state is released or greater. `Required` `Default(false)` `Filter(eq)` `ReadOnly` 
| [IsSingleExecution](Crm.Sales.SalesOrders.md#issingleexecution) | boolean | Specifies whether the document is a single execution of its order document. `Required` `Default(false)` `Filter(eq)` `ReadOnly` 
| [IsValidField](Crm.Sales.SalesOrders.md#isvalidfield) | boolean | True when the order is valid (e.g. released and not void). Used for internal processing. `Required` `Default(false)` `ReadOnly` 
| [Notes](Crm.Sales.SalesOrders.md#notes) | string (254) __nullable__ | Notes for this SalesOrder. 
| [ParentDocument<br />RelationshipType](Crm.Sales.SalesOrders.md#parentdocumentrelationshiptype) | [ParentDocument<br />RelationshipType](Crm.Sales.SalesOrders.md#parentdocumentrelationshiptype) __nullable__ | Type of relationship between the current document and the parent document(s). Affects the constraints for execution/completion for the documents. Possible values: 'S' = 'Subtask', 'N' = 'Next task'. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [PaymentDueDate](Crm.Sales.SalesOrders.md#paymentduedate) | datetime __nullable__ | The last term for the payment of the sales order. `Filter(ge;le)` 
| [PaymentDueStartDate](Crm.Sales.SalesOrders.md#paymentduestartdate) | datetime __nullable__ | The date when the payment becomes due for documents with one installment. null when the document is with multiple installments. 
| [PlanningOnly](Crm.Sales.SalesOrders.md#planningonly) | boolean | Indicates that the document is used only for planning (and as consequence its state cannot be greater than Planned). `Required` `Default(false)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [ReadOnly](Crm.Sales.SalesOrders.md#readonly) | boolean | True - the document is read only; false - the document is not read only. `Required` `Default(false)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [ReferenceDate](Crm.Sales.SalesOrders.md#referencedate) | datetime __nullable__ | The date to which this document refers, i.e. when the action really occurred. If null, Document_Date is taken. `Default(Today)` `Filter(ge;le)` (Inherited from [Documents](General.Documents.md)) 
| [ReferenceDocumentNo](Crm.Sales.SalesOrders.md#referencedocumentno) | string (20) __nullable__ | The number of the document (issued by the other party), which was the reason for the creation of the current document. The numebr should be unique within the party documents. `Filter(eq;like)` (Inherited from [Documents](General.Documents.md)) 
| [ReleaseTime](Crm.Sales.SalesOrders.md#releasetime) | datetime __nullable__ | Date and time when the document was released (State set to Released). `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [RequiredDeliveryDate](Crm.Sales.SalesOrders.md#requireddeliverydate) | date __nullable__ | The required delivery date for all lines in the sales order. Initially calculated, based on either the Ship To Customer or Customer delivery term. `Filter(ge;le)` 
| [State](Crm.Sales.SalesOrders.md#state) | [DocumentState](Crm.Sales.SalesOrders.md#state) | The current system state of the document. Allowed values: 0=New;5=Corrective;10=Computer Planned;20=Human Planned;30=Released;40=Completed;50=Closed. `Required` `Default(0)` `Filter(multi eq;ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [ToDate](Crm.Sales.SalesOrders.md#todate) | date __nullable__ | When selling a service valid only for a period, denotes the end of the period. null means that it is unknown or N/A. `Introduced in version 20.1` 
| [Void](Crm.Sales.SalesOrders.md#void) | boolean | True if the document is null and void. `Required` `Default(false)` `Filter(eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [VoidReason](Crm.Sales.SalesOrders.md#voidreason) | string (254) __nullable__ | Reason for voiding the document, entered by the user. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [VoidTime](Crm.Sales.SalesOrders.md#voidtime) | datetime __nullable__ | Date/time when the document has become void. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 
| [VoidUser](Crm.Sales.SalesOrders.md#voiduser) | string (64) __nullable__ | The user who voided the document. `ReadOnly` (Inherited from [Documents](General.Documents.md)) 

## References

| Name | Type | Description |
| ---- | ---- | --- |
| [AccessKey](Crm.Sales.SalesOrders.md#accesskey) | [AccessKeys](Systems.Security.AccessKeys.md) (nullable) | The access key, containing the user permissions for this document. null means that all users have unlimited permissions. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [AdjustedDocument](Crm.Sales.SalesOrders.md#adjusteddocument) | [Documents](General.Documents.md) (nullable) | The primary document, which the current document adjusts. null when this is not an adjustment document. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) |
| [AssignedToUser](Crm.Sales.SalesOrders.md#assignedtouser) | [Users](Systems.Security.Users.md) (nullable) | The user to which this document is assigned for handling. null means that the document is not assigned to specific user. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [CurrencyDirectory](Crm.Sales.SalesOrders.md#currencydirectory) | [CurrencyDirectories](General.CurrencyDirectories.md) (nullable) | The currency directory, containing all the convertion rates, used by the document. null means that the document does not need currency convertions. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [Customer](Crm.Sales.SalesOrders.md#customer) | [Customers](Crm.Customers.md) | The primary customer, which placed the sales order. `Required` `Filter(multi eq)` |
| [Deal](Crm.Sales.SalesOrders.md#deal) | [Deals](Crm.Presales.Deals.md) (nullable) | The opportunity (deal) on which this order is based. `Filter(multi eq)` |
| [Dealer](Crm.Sales.SalesOrders.md#dealer) | [Dealers](Crm.Dealers.md) (nullable) | The external dealer, associated with the sales order. `Filter(multi eq)` |
| [DealType](Crm.Sales.SalesOrders.md#dealtype) | [DealTypes](Finance.Vat.DealTypes.md) (nullable) | Deal type to be passed to the invoice. If deal type in entered then the invoice creates VAT entry for this deal type. `Filter(multi eq)` |
| [DistributionChannel](Crm.Sales.SalesOrders.md#distributionchannel) | [DistributionChannels](Crm.Marketing.DistributionChannels.md) (nullable) | The distribution channel, that is used to deliver the products. `Filter(multi eq)` |
| [DocumentCurrency](Crm.Sales.SalesOrders.md#documentcurrency) | [Currencies](General.Currencies.md) | The currency of the document; e.g. the currency of the amounts in the document. `Required` `Filter(multi eq)` |
| [DocumentType](Crm.Sales.SalesOrders.md#documenttype) | [DocumentTypes](General.DocumentTypes.md) | The user defined type of the document. Determines document behaviour, properties, additional amounts, validation, generations, etc. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [EndCustomerParty](Crm.Sales.SalesOrders.md#endcustomerparty) | [Parties](General.Contacts.Parties.md) (nullable) | The end customer is the customer of the dealer. It is stored for information purposes only. The end customer may not have customer definition; any party can be used. `Filter(multi eq)` `Introduced in version 20.1` |
| [EnterpriseCompany](Crm.Sales.SalesOrders.md#enterprisecompany) | [EnterpriseCompanies](General.EnterpriseCompanies.md) | The enterprise company which issued the document. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [EnterpriseCompanyLocation](Crm.Sales.SalesOrders.md#enterprisecompanylocation) | [CompanyLocations](General.Contacts.CompanyLocations.md) (nullable) | The enterprise company location which issued the document. null means that there is only one location within the enterprise company and locations are not used. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [FiscalPrinterPosDevice](Crm.Sales.SalesOrders.md#fiscalprinterposdevice) | [Devices](Crm.Pos.Devices.md) (nullable) | For POS sales, specifies the fiscal printer. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1` |
| [FromCompanyDivision](Crm.Sales.SalesOrders.md#fromcompanydivision) | [CompanyDivisions](General.Contacts.CompanyDivisions.md) (nullable) | The division of the company, issuing the document. null when the document is not issued by any specific division. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [FromParty](Crm.Sales.SalesOrders.md#fromparty) | [Parties](General.Contacts.Parties.md) | The party which issued the document. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [IntrastatTransportCountry](Crm.Sales.SalesOrders.md#intrastattransportcountry) | [Countries](General.Geography.Countries.md) (nullable) | Country of origin of the transport company; used for Intrastat reporting. `Filter(multi eq)` |
| [MasterDocument](Crm.Sales.SalesOrders.md#masterdocument) | [Documents](General.Documents.md) | In a multi-document tree, this is the root document, that created the whole tree. If this is the root it is equal to Id. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [Parent](Crm.Sales.SalesOrders.md#parent) | [Documents](General.Documents.md) (nullable) | In a multi-document tree, this is the direct parent document. If this is the root it is null. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [PaymentAccount](Crm.Sales.SalesOrders.md#paymentaccount) | [PaymentAccounts](Finance.Payments.PaymentAccounts.md) (nullable) | When not null, the payment account, where the payment is expected. null=no expectation for account. `Filter(multi eq)` |
| [PaymentType](Crm.Sales.SalesOrders.md#paymenttype) | [PaymentTypes](Finance.Payments.PaymentTypes.md) (nullable) | When not null specifies the payment type for the sales order. `Filter(multi eq)` |
| [PosLocation](Crm.Sales.SalesOrders.md#poslocation) | [Locations](Crm.Pos.Locations.md) (nullable) | For POS sales, specifies the POS location, in which the sale is performed. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1` |
| [PosOperator](Crm.Sales.SalesOrders.md#posoperator) | [Operators](Crm.Pos.Operators.md) (nullable) | For POS sales, specifies the POS operator, who created the sale. null when the sale is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1` |
| [PosTerminal](Crm.Sales.SalesOrders.md#posterminal) | [Terminals](Crm.Pos.Terminals.md) (nullable) | For POS sales, specifies the POS terminal, on which the sale is entered. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1` |
| [PriceList](Crm.Sales.SalesOrders.md#pricelist) | [PriceLists](Crm.PriceLists.md) (nullable) | The price list to be used for determining product prices in the lines. `Filter(multi eq)` |
| [PrimeCauseDocument](Crm.Sales.SalesOrders.md#primecausedocument) | [Documents](General.Documents.md) (nullable) | The document that is the prime cause for creation of the current document. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [ResponsiblePerson](Crm.Sales.SalesOrders.md#responsibleperson) | [Persons](General.Contacts.Persons.md) (nullable) | The person that is responsible for this order or transaction. It could be the sales person, the orderer, etc. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [ReturnForInvoice](Crm.Sales.SalesOrders.md#returnforinvoice) | [Invoices](Crm.Invoicing.Invoices.md) (nullable) | When specified indicates that some of the goods sold in the sales orders invoiced with Return_For_Invoice_Id are returned with the current document. `Filter(multi eq)` |
| [ReturnForSalesOrder](Crm.Sales.SalesOrders.md#returnforsalesorder) | [SalesOrders](Crm.Sales.SalesOrders.md) (nullable) | When specified indicates that some of the goods sold in Return_For_Sales_Order_Id are returned with the current document. `Filter(multi eq;like)` |
| [ReverseOfDocument](Crm.Sales.SalesOrders.md#reverseofdocument) | [Documents](General.Documents.md) (nullable) | The document which the current document is reverse of. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) |
| [SalesPerson](Crm.Sales.SalesOrders.md#salesperson) | [SalesPersons](Crm.SalesPersons.md) (nullable) | Internal company sales person. `Filter(multi eq)` |
| [Sequence](Crm.Sales.SalesOrders.md#sequence) | [Sequences](General.Sequences.md) (nullable) | The sequence that will be used to give new numbers to the documents of this type. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) |
| [ShipToCustomer](Crm.Sales.SalesOrders.md#shiptocustomer) | [Customers](Crm.Customers.md) (nullable) | The customer to whom to ship the sales order. Usually it is a customer entry for a sub-party of the primary customer. `Filter(multi eq)` |
| [ShipToPartyContact<br />Mechanism](Crm.Sales.SalesOrders.md#shiptopartycontactmechanism) | [PartyContactMechanisms](General.Contacts.PartyContactMechanisms.md) (nullable) | The contact mechanism (address) to whih to ship the sales order. `Filter(multi eq)` |
| [Store](Crm.Sales.SalesOrders.md#store) | [Stores](Logistics.Inventory.Stores.md) (nullable) | The store from which to issue the sales order. null means that there is no store associated with the sales order or there are different stores for some of the lines. `Filter(multi eq)` |
| [ToCompanyDivision](Crm.Sales.SalesOrders.md#tocompanydivision) | [CompanyDivisions](General.Contacts.CompanyDivisions.md) (nullable) | The division of the company, receiving the document. null when the document is not received by any specific division. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [ToParty](Crm.Sales.SalesOrders.md#toparty) | [Parties](General.Contacts.Parties.md) (nullable) | The party which should receive the document. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md)) |
| [UserStatus](Crm.Sales.SalesOrders.md#userstatus) | [DocumentTypeUserStatuses](General.DocumentTypeUserStatuses.md) (nullable) | The user status of this document if applicable for this document type. null means unknown or not yet set. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md)) |

## Child Collections

| Name | Type | Description |
| ---- | ---- | --- |
| Comments | [DocumentComments](General.DocumentComments.md) | List of `DocumentComment`(General.DocumentComments.md) child objects, based on the `General.DocumentComment.Document`(General.DocumentComments.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| DistributedAmounts | [DocumentDistributedAmounts](General.DocumentDistributedAmounts.md) | List of `DocumentDistributed<br />Amount`(General.DocumentDistributedAmounts.md) child objects, based on the `General.DocumentDistributedAmount.Document`(General.DocumentDistributedAmounts.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| DocumentAmounts | [DocumentAmounts](General.DocumentAmounts.md) | List of `DocumentAmount`(General.DocumentAmounts.md) child objects, based on the `General.DocumentAmount.Document`(General.DocumentAmounts.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| FileAttachments | [DocumentFileAttachments](General.DocumentFileAttachments.md) | List of `DocumentFileAttachment`(General.DocumentFileAttachments.md) child objects, based on the `General.DocumentFileAttachment.Document`(General.DocumentFileAttachments.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| LineAmounts | [DocumentLineAmounts](General.DocumentLineAmounts.md) | List of `DocumentLineAmount`(General.DocumentLineAmounts.md) child objects, based on the `General.DocumentLineAmount.Document`(General.DocumentLineAmounts.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| Lines | [SalesOrderLines](Crm.Sales.SalesOrderLines.md) | List of `SalesOrderLine`(Crm.Sales.SalesOrderLines.md) child objects, based on the `Crm.Sales.SalesOrderLine.SalesOrder`(Crm.Sales.SalesOrderLines.md#salesorder) back reference 
| PaymentPlans | [SalesOrderPaymentPlans](Crm.Sales.SalesOrderPaymentPlans.md) | List of `SalesOrderPaymentPlan`(Crm.Sales.SalesOrderPaymentPlans.md) child objects, based on the `Crm.Sales.SalesOrderPaymentPlan.SalesOrder`(Crm.Sales.SalesOrderPaymentPlans.md#salesorder) back reference 
| Prints | [DocumentPrints](General.DocumentPrints.md) | List of `DocumentPrint`(General.DocumentPrints.md) child objects, based on the `General.DocumentPrint.Document`(General.DocumentPrints.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| PromotionalPackages | [SalesOrderPromotionalPackages](Crm.Sales.SalesOrderPromotionalPackages.md) | List of `SalesOrderPromotional<br />Package`(Crm.Sales.SalesOrderPromotional<br />Packages.md) child objects, based on the `Crm.Sales.SalesOrderPromotional<br />Package.SalesOrder`(Crm.Sales.SalesOrderPromotional<br />Packages.md#salesorder) back reference 
| StateChanges | [DocumentStateChanges](General.DocumentStateChanges.md) | List of `DocumentStateChange`(General.DocumentStateChanges.md) child objects, based on the `General.DocumentStateChange.Document`(General.DocumentStateChanges.md#document) back reference (Inherited from [Documents](General.Documents.md)) 
| Versions | [DocumentVersions](General.DocumentVersions.md) | List of `DocumentVersion`(General.DocumentVersions.md) child objects, based on the `General.DocumentVersion.Document`(General.DocumentVersions.md#document) back reference (Inherited from [Documents](General.Documents.md)) 


## Attribute Details

### AdjustmentNumber

Consecutive number of the correction that this document is applying to the adjusted document. `Required` `Default(0)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **int32**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Default Value_: **0**  

### AdjustmentTime

Date/time when the document last has been adjusted by corrective document. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

### AdjustmentUser

The user who adjusted the document. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **string (64) __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Maximum Length_: **64**  

### CompleteTime

Date and time when the document was completed (State set to Completed). `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  

### CreationTime

Date/Time when the document was created. `Required` `Default(Now)` `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  
_Default Value_: **CurrentDateTime**  

### CreationUser

The login name of the user, who created the document. `Required` `Filter(like)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **string (64)**  
_Supported Filters_: **Like**  
_Supports Order By_: **False**  
_Maximum Length_: **64**  

### CreditLimitOverride

Allows the sales order to be released even in the case of violations of credit limit or presence of overdue receivables. `Required` `Default(false)` `Filter(eq)`

_Type_: **boolean**  
_Supported Filters_: **Equals**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### CustomerPurchaseOrderDate

Issue date of the referent customer purchase order. `Filter(ge;le)`

_Type_: **date __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  

### CustomerPurchaseOrderNo

Reference number of the customer's purchase order. `Filter(eq;like)`

_Type_: **string (20) __nullable__**  
_Supported Filters_: **Equals, Like**  
_Supports Order By_: **False**  
_Maximum Length_: **20**  

### DeliveryTermsCode

Mode of delivery, like CIF, FOB, etc. Used also in Intrastat reporting.

_Type_: **[DeliveryTerms](Crm.Sales.SalesOrders.md#deliverytermscode) __nullable__**  
Generic enum type for DeliveryTerms properties  
_Allowed Values (Finance.Intrastat.DeliveryTerms Enum Members)_  

| Value | Description |
| ---- | --- |
| ExWorks | ExWorks value. Stored as 'EXW'. <br /> _Database Value:_ 'EXW' <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'ExWorks' |
| FrancoCarrier | FrancoCarrier value. Stored as 'FCA'. <br /> _Database Value:_ 'FCA' <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'FrancoCarrier' |
| FreeAlongsideShip | FreeAlongsideShip value. Stored as 'FAS'. <br /> _Database Value:_ 'FAS' <br /> _Model Value:_ 2 <br /> _Domain API Value:_ 'FreeAlongsideShip' |
| FreeOnBoard | FreeOnBoard value. Stored as 'FOB'. <br /> _Database Value:_ 'FOB' <br /> _Model Value:_ 3 <br /> _Domain API Value:_ 'FreeOnBoard' |
| CostAndFreightCF | CostAndFreightCF value. Stored as 'CFR'. <br /> _Database Value:_ 'CFR' <br /> _Model Value:_ 4 <br /> _Domain API Value:_ 'CostAndFreightCF' |
| CostInsuranceAndFreight | CostInsuranceAndFreight value. Stored as 'CIF'. <br /> _Database Value:_ 'CIF' <br /> _Model Value:_ 5 <br /> _Domain API Value:_ 'CostInsuranceAndFreight' |
| CarriagePaidTo | CarriagePaidTo value. Stored as 'CPT'. <br /> _Database Value:_ 'CPT' <br /> _Model Value:_ 6 <br /> _Domain API Value:_ 'CarriagePaidTo' |
| CarriageAndInsurancePaidTo | CarriageAndInsurancePaidTo value. Stored as 'CIP'. <br /> _Database Value:_ 'CIP' <br /> _Model Value:_ 7 <br /> _Domain API Value:_ 'CarriageAndInsurancePaidTo' |
| DeliveredAtPlace | DeliveredAtPlace value. Stored as 'DAP'. <br /> _Database Value:_ 'DAP' <br /> _Model Value:_ 8 <br /> _Domain API Value:_ 'DeliveredAtPlace' |
| DeliveredAtTerminal | DeliveredAtTerminal value. Stored as 'DAT'. <br /> _Database Value:_ 'DAT' <br /> _Model Value:_ 9 <br /> _Domain API Value:_ 'DeliveredAtTerminal' |
| DeliveredDutyPaid | DeliveredDutyPaid value. Stored as 'DDP'. <br /> _Database Value:_ 'DDP' <br /> _Model Value:_ 10 <br /> _Domain API Value:_ 'DeliveredDutyPaid' |
| DeliveredAtPlaceUnloaded | DeliveredAtPlaceUnloaded value. Stored as 'DPU'. <br /> _Database Value:_ 'DPU' <br /> _Model Value:_ 11 <br /> _Domain API Value:_ 'DeliveredAtPlaceUnloaded' |

_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => SalesOrderLinesRepository.DeliveryTermsCodeAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### DocumentDate

The date on which the document was issued. `Required` `Default(Today)` `Filter(eq;ge;le)` `ORD` (Inherited from [Documents](General.Documents.md))

_Type_: **date**  
_Indexed_: **True**  
_Supported Filters_: **Equals, GreaterThanOrLessThan**  
_Supports Order By_: **True**  
_Default Value_: **CurrentDate**  

### DocumentNo

Document number, unique within Document_Type_Id. `Required` `Filter(eq;like)` `ORD` (Inherited from [Documents](General.Documents.md))

_Type_: **string (20)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, Like**  
_Supports Order By_: **True**  
_Maximum Length_: **20**  

### DocumentNotes

Notes for this Document. (Inherited from [Documents](General.Documents.md))

_Type_: **string (max) __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Maximum Length_: **2147483647**  

### DocumentVersion

Consecutive version number, starting with 1. Each update produces a new version of the document. `Required` `Default(1)` `Filter(eq;ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **int32**  
_Supported Filters_: **Equals, GreaterThanOrLessThan**  
_Supports Order By_: **False**  
_Default Value_: **1**  

### EntityName

The entity name of the document header. `Required` `Filter(eq)` `ORD` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **string (64)**  
_Indexed_: **True**  
_Supported Filters_: **Equals**  
_Supports Order By_: **True**  
_Maximum Length_: **64**  

### FiscalSalesNumber

Unique number of the sale, assigned for fiscal reporting purposes. The format is according to the applicable legislation. null means that there is no requirement for fiscal sales number for this document or it is unknown. `Filter(multi eq;like)` `ReadOnly` `Introduced in version 19.1`

_Type_: **string (32) __nullable__**  
_Supported Filters_: **Equals, Like, EqualsIn**  
_Supports Order By_: **False**  
_Maximum Length_: **32**  

### FromDate

When selling a service valid only for a period, denotes the beginning of the period. null means that it is unknown or N/A. `Introduced in version 20.1`

_Type_: **date __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => SalesOrderLinesRepository.LineFromDateAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### Id

_Type_: **guid**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  
_Default Value_: **NewGuid**  

### IntrastatTransactionNatureCode

Transaction nature; used for Intrastat reporting.

_Type_: **[TransactionNature](Crm.Sales.SalesOrders.md#intrastattransactionnaturecode) __nullable__**  
Generic enum type for TransactionNature properties  
_Allowed Values (Finance.Intrastat.TransactionNature Enum Members)_  

| Value | Description |
| ---- | --- |
| OutrightPurchaseOrSale | OutrightPurchaseOrSale value. Stored as '11'. <br /> _Database Value:_ '11' <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'OutrightPurchaseOrSale' |
| SupplyForSale | SupplyForSale value. Stored as '12'. <br /> _Database Value:_ '12' <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'SupplyForSale' |
| BarterTrade | BarterTrade value. Stored as '13'. <br /> _Database Value:_ '13' <br /> _Model Value:_ 2 <br /> _Domain API Value:_ 'BarterTrade' |
| FinancialLeasing | FinancialLeasing value. Stored as '14'. <br /> _Database Value:_ '14' <br /> _Model Value:_ 3 <br /> _Domain API Value:_ 'FinancialLeasing' |
| OtherTransactions | OtherTransactions value. Stored as '19'. <br /> _Database Value:_ '19' <br /> _Model Value:_ 4 <br /> _Domain API Value:_ 'OtherTransactions' |
| ReturnStokilizing | ReturnStokilizing value. Stored as '21'. <br /> _Database Value:_ '21' <br /> _Model Value:_ 5 <br /> _Domain API Value:_ 'ReturnStokilizing' |
| ReplacementFor<br />ReturnedGoods | ReplacementFor<br />ReturnedGoods value. Stored as '22'. <br /> _Database Value:_ '22' <br /> _Model Value:_ 6 <br /> _Domain API Value:_ 'ReplacementFor<br />ReturnedGoods' |
| ReplacementOfGoods<br />NotBeingReturned | ReplacementOfGoods<br />NotBeingReturned value. Stored as '23'. <br /> _Database Value:_ '23' <br /> _Model Value:_ 7 <br /> _Domain API Value:_ 'ReplacementOfGoods<br />NotBeingReturned' |
| ReturnOrExchange<br />OfOtherGoods | ReturnOrExchange<br />OfOtherGoods value. Stored as '29'. <br /> _Database Value:_ '29' <br /> _Model Value:_ 8 <br /> _Domain API Value:_ 'ReturnOrExchange<br />OfOtherGoods' |
| SpecificTransactions | SpecificTransactions value. Stored as '60'. <br /> _Database Value:_ '60' <br /> _Model Value:_ 9 <br /> _Domain API Value:_ 'SpecificTransactions' |
| OperationsOnJointProjects | OperationsOnJointProjects value. Stored as '70'. <br /> _Database Value:_ '70' <br /> _Model Value:_ 10 <br /> _Domain API Value:_ 'OperationsOnJointProjects' |
| TransactionsOf<br />ConstructionMaterials<br />AndEquipment | TransactionsOf<br />ConstructionMaterials<br />AndEquipment value. Stored as '80'. <br /> _Database Value:_ '80' <br /> _Model Value:_ 11 <br /> _Domain API Value:_ 'TransactionsOf<br />ConstructionMaterials<br />AndEquipment' |
| OtherTransactionsLeasing | OtherTransactionsLeasing value. Stored as '91'. <br /> _Database Value:_ '91' <br /> _Model Value:_ 12 <br /> _Domain API Value:_ 'OtherTransactionsLeasing' |
| OtherTransactionsOther | OtherTransactionsOther value. Stored as '99'. <br /> _Database Value:_ '99' <br /> _Model Value:_ 13 <br /> _Domain API Value:_ 'OtherTransactionsOther' |
| DealsThatInclude<br />PropertyTransfers<br />WithoutFinancial<br />CompensationOr<br />CompensationIn<br />Kind | DealsThatInclude<br />PropertyTransfers<br />WithoutFinancial<br />CompensationOr<br />CompensationIn<br />Kind value. Stored as '30'. <br /> _Database Value:_ '30' <br /> _Model Value:_ 14 <br /> _Domain API Value:_ 'DealsThatInclude<br />PropertyTransfers<br />WithoutFinancial<br />CompensationOr<br />CompensationIn<br />Kind' |
| GoodsThatAreExpected<br />ToBeReturnedTo<br />Sender | GoodsThatAreExpected<br />ToBeReturnedTo<br />Sender value. Stored as '41'. <br /> _Database Value:_ '41' <br /> _Model Value:_ 15 <br /> _Domain API Value:_ 'GoodsThatAreExpected<br />ToBeReturnedTo<br />Sender' |
| GoodsThatAreNot<br />ExpectedToBeReturned<br />ToSender | GoodsThatAreNot<br />ExpectedToBeReturned<br />ToSender value. Stored as '42'. <br /> _Database Value:_ '42' <br /> _Model Value:_ 16 <br /> _Domain API Value:_ 'GoodsThatAreNot<br />ExpectedToBeReturned<br />ToSender' |
| GoodsThatAreReturned<br />ToSender | GoodsThatAreReturned<br />ToSender value. Stored as '51'. <br /> _Database Value:_ '51' <br /> _Model Value:_ 17 <br /> _Domain API Value:_ 'GoodsThatAreReturned<br />ToSender' |
| GoodsThatAreNot<br />ReturnedToSender | GoodsThatAreNot<br />ReturnedToSender value. Stored as '52'. <br /> _Database Value:_ '52' <br /> _Model Value:_ 18 <br /> _Domain API Value:_ 'GoodsThatAreNot<br />ReturnedToSender' |

_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => SalesOrderLinesRepository.IntrastatTransactionNatureCodeAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### IntrastatTransportModeCode

Transport mode; used for Intrastat reporting.

_Type_: **[TransportMode](Crm.Sales.SalesOrders.md#intrastattransportmodecode) __nullable__**  
Generic enum type for TransportMode properties  
_Allowed Values (Finance.Intrastat.TransportMode Enum Members)_  

| Value | Description |
| ---- | --- |
| Shipping | Shipping value. Stored as '1'. <br /> _Database Value:_ '1' <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'Shipping' |
| RailwayTransport | RailwayTransport value. Stored as '2'. <br /> _Database Value:_ '2' <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'RailwayTransport' |
| RoadTransport | RoadTransport value. Stored as '3'. <br /> _Database Value:_ '3' <br /> _Model Value:_ 2 <br /> _Domain API Value:_ 'RoadTransport' |
| AirTransport | AirTransport value. Stored as '4'. <br /> _Database Value:_ '4' <br /> _Model Value:_ 3 <br /> _Domain API Value:_ 'AirTransport' |
| Mail | Mail value. Stored as '5'. <br /> _Database Value:_ '5' <br /> _Model Value:_ 4 <br /> _Domain API Value:_ 'Mail' |
| FixedTransport<br />Installations | FixedTransport<br />Installations value. Stored as '6'. <br /> _Database Value:_ '6' <br /> _Model Value:_ 5 <br /> _Domain API Value:_ 'FixedTransport<br />Installations' |
| RiverTransport | RiverTransport value. Stored as '7'. <br /> _Database Value:_ '7' <br /> _Model Value:_ 6 <br /> _Domain API Value:_ 'RiverTransport' |
| SelfPropelled | SelfPropelled value. Stored as '8'. <br /> _Database Value:_ '8' <br /> _Model Value:_ 7 <br /> _Domain API Value:_ 'SelfPropelled' |

_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => SalesOrderLinesRepository.IntrastatTransportModeCodeAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### IsReleased

True if the document is not void and its state is released or greater. `Required` `Default(false)` `Filter(eq)` `ReadOnly`

_Type_: **boolean**  
_Supported Filters_: **Equals**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### IsSingleExecution

Specifies whether the document is a single execution of its order document. `Required` `Default(false)` `Filter(eq)` `ReadOnly`

_Type_: **boolean**  
_Supported Filters_: **Equals**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### IsValidField

True when the order is valid (e.g. released and not void). Used for internal processing. `Required` `Default(false)` `ReadOnly`

_Type_: **boolean**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### Notes

Notes for this SalesOrder.

_Type_: **string (254) __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Maximum Length_: **254**  

### ParentDocumentRelationshipType

Type of relationship between the current document and the parent document(s). Affects the constraints for execution/completion for the documents. Possible values: 'S' = 'Subtask', 'N' = 'Next task'. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[ParentDocument<br />RelationshipType](Crm.Sales.SalesOrders.md#parentdocumentrelationshiptype) __nullable__**  
Relationship between parent and child documents  
_Allowed Values (General.ParentDocumentRelationshipType Enum Members)_  

| Value | Description |
| ---- | --- |
| Subtask | The child document is a sub-task of the parent document. (Complete child to complete parent) <br /> _Database Value:_ 'S' <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'Subtask' |
| NextTask | The child document is next task of the parent document. (Complete parent to complete child) <br /> _Database Value:_ 'N' <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'NextTask' |

_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

### PaymentDueDate

The last term for the payment of the sales order. `Filter(ge;le)`

_Type_: **datetime __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.DocumentDate.AddDays( Convert( IIF( ( obj.ShipToCustomer.DefaultPaymentTermDays != 0), obj.ShipToCustomer.DefaultPaymentTermDays, obj.Customer.DefaultPaymentTermDays), Double))`
### PaymentDueStartDate

The date when the payment becomes due for documents with one installment. null when the document is with multiple installments.

_Type_: **datetime __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.DocumentDate.AddDays( Convert( IIF( ( obj.ShipToCustomer.DefaultPaymentStartDays != 0), obj.ShipToCustomer.DefaultPaymentStartDays, obj.Customer.DefaultPaymentStartDays), Double))`
### PlanningOnly

Indicates that the document is used only for planning (and as consequence its state cannot be greater than Planned). `Required` `Default(false)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **boolean**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### ReadOnly

True - the document is read only; false - the document is not read only. `Required` `Default(false)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **boolean**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### ReferenceDate

The date to which this document refers, i.e. when the action really occurred. If null, Document_Date is taken. `Default(Today)` `Filter(ge;le)` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  
_Default Value_: **CurrentDate**  

### ReferenceDocumentNo

The number of the document (issued by the other party), which was the reason for the creation of the current document. The numebr should be unique within the party documents. `Filter(eq;like)` (Inherited from [Documents](General.Documents.md))

_Type_: **string (20) __nullable__**  
_Supported Filters_: **Equals, Like**  
_Supports Order By_: **False**  
_Maximum Length_: **20**  

### ReleaseTime

Date and time when the document was released (State set to Released). `Filter(ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  

### RequiredDeliveryDate

The required delivery date for all lines in the sales order. Initially calculated, based on either the Ship To Customer or Customer delivery term. `Filter(ge;le)`

_Type_: **date __nullable__**  
_Supported Filters_: **GreaterThanOrLessThan**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.DocumentDate.AddDays( Convert( IIF( ( obj.ShipToCustomer.DefaultDeliveryTermDays != 0), obj.ShipToCustomer.DefaultDeliveryTermDays, obj.Customer.DefaultDeliveryTermDays), Double))`
`obj.Lines.Select( c => SalesOrderLinesRepository.RequiredDeliveryDateAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### State

The current system state of the document. Allowed values: 0=New;5=Corrective;10=Computer Planned;20=Human Planned;30=Released;40=Completed;50=Closed. `Required` `Default(0)` `Filter(multi eq;ge;le)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[DocumentState](Crm.Sales.SalesOrders.md#state)**  
Enumeration of document system states  
_Allowed Values (General.DocumentState Enum Members)_  

| Value | Description |
| ---- | --- |
| New | New document, just created. Can be edited. (Stored as 0). <br /> _Database Value:_ 0 <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'New' |
| Adjustment | Document which adjusts other released documents. (Stored as 5). <br /> _Database Value:_ 5 <br /> _Model Value:_ 5 <br /> _Domain API Value:_ 'Adjustment' |
| Planned | Planned by the system for future releasing. (Stored as 10). <br /> _Database Value:_ 10 <br /> _Model Value:_ 10 <br /> _Domain API Value:_ 'Planned' |
| FirmPlanned | Planned by operator for future releasing. (Stored as 20). <br /> _Database Value:_ 20 <br /> _Model Value:_ 20 <br /> _Domain API Value:_ 'FirmPlanned' |
| Released | Released document. Changes can be applied only through adjustment documents. (Stored as 30). <br /> _Database Value:_ 30 <br /> _Model Value:_ 30 <br /> _Domain API Value:_ 'Released' |
| Completed | Work has completed. (Stored as 40). <br /> _Database Value:_ 40 <br /> _Model Value:_ 40 <br /> _Domain API Value:_ 'Completed' |
| Closed | The document is audited and closed. Adjustments are not allowed, but reopening is allowed. (Stored as 50). <br /> _Database Value:_ 50 <br /> _Model Value:_ 50 <br /> _Domain API Value:_ 'Closed' |

_Supported Filters_: **Equals, GreaterThanOrLessThan, EqualsIn**  
_Supports Order By_: **False**  
_Default Value_: **0**  

### ToDate

When selling a service valid only for a period, denotes the end of the period. null means that it is unknown or N/A. `Introduced in version 20.1`

_Type_: **date __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => SalesOrderLinesRepository.LineToDateAttribute.GetUntypedValue( c, False)).Distinct( ).OnlyIfSingle( )`
### Void

True if the document is null and void. `Required` `Default(false)` `Filter(eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **boolean**  
_Indexed_: **True**  
_Supported Filters_: **Equals**  
_Supports Order By_: **False**  
_Default Value_: **False**  

### VoidReason

Reason for voiding the document, entered by the user. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **string (254) __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Maximum Length_: **254**  

### VoidTime

Date/time when the document has become void. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **datetime __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  

### VoidUser

The user who voided the document. `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **string (64) __nullable__**  
_Supported Filters_: **NotFilterable**  
_Supports Order By_: **False**  
_Maximum Length_: **64**  


## Reference Details

### AccessKey

The access key, containing the user permissions for this document. null means that all users have unlimited permissions. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[AccessKeys](Systems.Security.AccessKeys.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### AdjustedDocument

The primary document, which the current document adjusts. null when this is not an adjustment document. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[Documents](General.Documents.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### AssignedToUser

The user to which this document is assigned for handling. null means that the document is not assigned to specific user. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Users](Systems.Security.Users.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### CurrencyDirectory

The currency directory, containing all the convertion rates, used by the document. null means that the document does not need currency convertions. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[CurrencyDirectories](General.CurrencyDirectories.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### Customer

The primary customer, which placed the sales order. `Required` `Filter(multi eq)`

_Type_: **[Customers](Crm.Customers.md)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### Deal

The opportunity (deal) on which this order is based. `Filter(multi eq)`

_Type_: **[Deals](Crm.Presales.Deals.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### Dealer

The external dealer, associated with the sales order. `Filter(multi eq)`

_Type_: **[Dealers](Crm.Dealers.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### DealType

Deal type to be passed to the invoice. If deal type in entered then the invoice creates VAT entry for this deal type. `Filter(multi eq)`

_Type_: **[DealTypes](Finance.Vat.DealTypes.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => c.LineDealType).Distinct( ).OnlyIfSingle( )`
### DistributionChannel

The distribution channel, that is used to deliver the products. `Filter(multi eq)`

_Type_: **[DistributionChannels](Crm.Marketing.DistributionChannels.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.ShipToCustomer.DefaultDistributionChannel.IfNullThen( obj.Customer.DefaultDistributionChannel).IfNullThen( obj.DistributionChannel)`
### DocumentCurrency

The currency of the document; e.g. the currency of the amounts in the document. `Required` `Filter(multi eq)`

_Type_: **[Currencies](General.Currencies.md)**  
_Supported Filters_: **Equals, EqualsIn**  

_Back-End Default Expression:_  
`obj.ShipToCustomer.DefaultCurrency.IfNullThen( obj.Customer.DefaultCurrency).IfNullThen( obj.DocumentCurrency).IfNullThen( obj.EnterpriseCompany.BaseCurrency)`

_Front-End Recalc Expressions:_  
`obj.ShipToCustomer.DefaultCurrency.IfNullThen( obj.Customer.DefaultCurrency).IfNullThen( obj.DocumentCurrency).IfNullThen( obj.EnterpriseCompany.BaseCurrency)`
### DocumentType

The user defined type of the document. Determines document behaviour, properties, additional amounts, validation, generations, etc. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[DocumentTypes](General.DocumentTypes.md)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### EndCustomerParty

The end customer is the customer of the dealer. It is stored for information purposes only. The end customer may not have customer definition; any party can be used. `Filter(multi eq)` `Introduced in version 20.1`

_Type_: **[Parties](General.Contacts.Parties.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => c.LineEndCustomerParty).Distinct( ).OnlyIfSingle( )`
### EnterpriseCompany

The enterprise company which issued the document. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[EnterpriseCompanies](General.EnterpriseCompanies.md)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### EnterpriseCompanyLocation

The enterprise company location which issued the document. null means that there is only one location within the enterprise company and locations are not used. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[CompanyLocations](General.Contacts.CompanyLocations.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### FiscalPrinterPosDevice

For POS sales, specifies the fiscal printer. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1`

_Type_: **[Devices](Crm.Pos.Devices.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### FromCompanyDivision

The division of the company, issuing the document. null when the document is not issued by any specific division. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[CompanyDivisions](General.Contacts.CompanyDivisions.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### FromParty

The party which issued the document. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Parties](General.Contacts.Parties.md)**  
_Supported Filters_: **Equals, EqualsIn**  

### IntrastatTransportCountry

Country of origin of the transport company; used for Intrastat reporting. `Filter(multi eq)`

_Type_: **[Countries](General.Geography.Countries.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => c.IntrastatTransportCountry).Distinct( ).OnlyIfSingle( )`
### MasterDocument

In a multi-document tree, this is the root document, that created the whole tree. If this is the root it is equal to Id. `Required` `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Documents](General.Documents.md)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### Parent

In a multi-document tree, this is the direct parent document. If this is the root it is null. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Documents](General.Documents.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### PaymentAccount

When not null, the payment account, where the payment is expected. null=no expectation for account. `Filter(multi eq)`

_Type_: **[PaymentAccounts](Finance.Payments.PaymentAccounts.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`DeterminePaymentAccount( obj.Transaction, obj.Customer, obj.ShipToCustomer, obj.PaymentType, obj.PaymentAccount)`
`obj.PaymentPlans.Select( c => c.PaymentAccount).Distinct( ).OnlyIfSingle( )`
### PaymentType

When not null specifies the payment type for the sales order. `Filter(multi eq)`

_Type_: **[PaymentTypes](Finance.Payments.PaymentTypes.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.ShipToCustomer.DefaultPaymentType.IfNullThen( obj.Customer.DefaultPaymentType).IfNullThen( obj.PaymentType)`
`obj.PaymentPlans.Select( c => c.PaymentType).Distinct( ).OnlyIfSingle( )`
### PosLocation

For POS sales, specifies the POS location, in which the sale is performed. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1`

_Type_: **[Locations](Crm.Pos.Locations.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### PosOperator

For POS sales, specifies the POS operator, who created the sale. null when the sale is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1`

_Type_: **[Operators](Crm.Pos.Operators.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### PosTerminal

For POS sales, specifies the POS terminal, on which the sale is entered. null when the sales is not a POS sale. `Filter(multi eq)` `Introduced in version 19.1`

_Type_: **[Terminals](Crm.Pos.Terminals.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### PriceList

The price list to be used for determining product prices in the lines. `Filter(multi eq)`

_Type_: **[PriceLists](Crm.PriceLists.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### PrimeCauseDocument

The document that is the prime cause for creation of the current document. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Documents](General.Documents.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### ResponsiblePerson

The person that is responsible for this order or transaction. It could be the sales person, the orderer, etc. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Persons](General.Contacts.Persons.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### ReturnForInvoice

When specified indicates that some of the goods sold in the sales orders invoiced with Return_For_Invoice_Id are returned with the current document. `Filter(multi eq)`

_Type_: **[Invoices](Crm.Invoicing.Invoices.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

### ReturnForSalesOrder

When specified indicates that some of the goods sold in Return_For_Sales_Order_Id are returned with the current document. `Filter(multi eq;like)`

_Type_: **[SalesOrders](Crm.Sales.SalesOrders.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, Like, EqualsIn**  

### ReverseOfDocument

The document which the current document is reverse of. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[Documents](General.Documents.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### SalesPerson

Internal company sales person. `Filter(multi eq)`

_Type_: **[SalesPersons](Crm.SalesPersons.md) (nullable)**  
_Indexed_: **True**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`DetermineSalesPerson( obj.Transaction, obj.EnterpriseCompany, new [] {obj.Customer, obj.ShipToCustomer})`
### Sequence

The sequence that will be used to give new numbers to the documents of this type. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[Sequences](General.Sequences.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### ShipToCustomer

The customer to whom to ship the sales order. Usually it is a customer entry for a sub-party of the primary customer. `Filter(multi eq)`

_Type_: **[Customers](Crm.Customers.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`DetermineShipToCustomer( obj.Transaction, obj.EnterpriseCompany, obj.EnterpriseCompanyLocation, obj.Customer, obj.ShipToCustomer)`
### ShipToPartyContactMechanism

The contact mechanism (address) to whih to ship the sales order. `Filter(multi eq)`

_Type_: **[PartyContactMechanisms](General.Contacts.PartyContactMechanisms.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.ShipToCustomer.Party.DefaultContactMechanism.IfNullThen( obj.Customer.Party.DefaultContactMechanism)`
### Store

The store from which to issue the sales order. null means that there is no store associated with the sales order or there are different stores for some of the lines. `Filter(multi eq)`

_Type_: **[Stores](Logistics.Inventory.Stores.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

_Front-End Recalc Expressions:_  
`obj.Lines.Select( c => c.LineStore).Distinct( ).OnlyIfSingle( )`
### ToCompanyDivision

The division of the company, receiving the document. null when the document is not received by any specific division. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[CompanyDivisions](General.Contacts.CompanyDivisions.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### ToParty

The party which should receive the document. `Filter(multi eq)` (Inherited from [Documents](General.Documents.md))

_Type_: **[Parties](General.Contacts.Parties.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  

### UserStatus

The user status of this document if applicable for this document type. null means unknown or not yet set. `Filter(multi eq)` `ReadOnly` (Inherited from [Documents](General.Documents.md))

_Type_: **[DocumentTypeUserStatuses](General.DocumentTypeUserStatuses.md) (nullable)**  
_Supported Filters_: **Equals, EqualsIn**  


## API Methods

Methods that can be invoked in public APIs.

### ChangeState

Changes the document state to the specified new state              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **void**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **POST**  

**Parameters**  
  * **newState**  
    The desired new state of the document  
    _Type_: General.DocumentState  
    Enumeration of document system states  
    _Allowed Values (General.DocumentState Enum Members)_  

    | Value | Description |
    | ---- | --- |
    | New | New document, just created. Can be edited. (Stored as 0). <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'New' |
    | Adjustment | Document which adjusts other released documents. (Stored as 5). <br /> _Model Value:_ 5 <br /> _Domain API Value:_ 'Adjustment' |
    | Planned | Planned by the system for future releasing. (Stored as 10). <br /> _Model Value:_ 10 <br /> _Domain API Value:_ 'Planned' |
    | FirmPlanned | Planned by operator for future releasing. (Stored as 20). <br /> _Model Value:_ 20 <br /> _Domain API Value:_ 'FirmPlanned' |
    | Released | Released document. Changes can be applied only through adjustment documents. (Stored as 30). <br /> _Model Value:_ 30 <br /> _Domain API Value:_ 'Released' |
    | Completed | Work has completed. (Stored as 40). <br /> _Model Value:_ 40 <br /> _Domain API Value:_ 'Completed' |
    | Closed | The document is audited and closed. Adjustments are not allowed, but reopening is allowed. (Stored as 50). <br /> _Model Value:_ 50 <br /> _Domain API Value:_ 'Closed' |


  * **userStatus**  
    The desired new user status of the document. Can be null.  
    _Type_: [DocumentTypeUserStatuses](General.DocumentTypeUserStatuses.md)  
     _Optional_: True  
    _Default Value_: null  


The process of changing the document state is very labor intensive and includes              validation, generation of sub-documents and some other document-specific tasks.                          The state changing process might be very time-consuming, usually ranging              from 500 to 5000 milliseconds.                          Document states usually can only be advanced to higher states, but there are              exceptions from this rule. Database settings and configuration options might affect             the allowed state changes.                          Numerous kinds of document-specific and generic exceptions can be thrown during the             process.

### Complete

Changes the document state to Completed with all Release-ed sub-documents              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **void**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **POST**  

**Parameters**  
  * **completion**  
    How the sub-documents will be completed, if at all  
    _Type_: General.DocumentCompletion  
    Determines how Document will be completed  
    _Allowed Values (General.DocumentCompletion Enum Members)_  

    | Value | Description |
    | ---- | --- |
    | OnlyDocument | Only the document will be completed <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'OnlyDocument' |
    | WithAllChildren | The document, along with is sub-documents, will be completed <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'WithAllChildren' |
    | WithReleasedChildren | The document, along with its Release-ed sub-documents, will be completed <br /> _Model Value:_ 2 <br /> _Domain API Value:_ 'WithReleasedChildren' |



The process of changing the document state is very labor intensive and includes              validation, generation of sub-documents and some other document-specific tasks.                          The state changing process might be very time-consuming, usually ranging              from 500 to 5000 milliseconds.                          Document states usually can only be advanced to higher states, but there are              exceptions from this rule. Database settings and configuration options might affect             the allowed state changes.                          Numerous kinds of document-specific and generic exceptions can be thrown during the             process.

### MakeVoid

Makes the document void. The operation is irreversible.              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **void**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **POST**  

**Parameters**  
  * **reason**  
    The reason for voiding the document.  
    _Type_: string  

  * **voidType**  
    The type of void operation to execute.  
    _Type_: General.DocumentsRepositoryBase.VoidType  
    Specifies the type of void operation  
    _Allowed Values (General.DocumentsRepositoryBase.VoidType Enum Members)_  

    | Value | Description |
    | ---- | --- |
    | VoidDocument | Void only the document. If there are sub-documents, the operation might fail. <br /> _Model Value:_ 0 <br /> _Domain API Value:_ 'VoidDocument' |
    | VoidWithSubDocuments | Void the document and its non-released sub-documents. If there are released sub-documents, the operation might fail. <br /> _Model Value:_ 1 <br /> _Domain API Value:_ 'VoidWithSubDocuments' |
    | VoidWithReleased<br />SubDocuments | Void the document and all of its sub-documents, even the released ones. <br /> _Model Value:_ 2 <br /> _Domain API Value:_ 'VoidWithReleased<br />SubDocuments' |

     _Optional_: True  
    _Default Value_: VoidDocument  


### GetPrintout

Gets a document printout as a file. The returned value is Base64 string representation of the file contents.              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **string**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **POST**  

**Parameters**  
  * **fileFormat**  
    The file format: pdf, html, xlsx, xls, docx, txt and png. The default format is 'pdf'.  
    _Type_: string  
     _Optional_: True  
    _Default Value_: pdf  

  * **printout**  
    The printout defined for the document type of the document. If null the default printout of the document type is used.  
    _Type_: [Printouts](General.Printouts.md)  
     _Optional_: True  
    _Default Value_: null  


### Recalculate

The document and all of its owned objects will be altered to become valid.              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **void**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **POST**  

In some cases the objects in child collection of the document depend on values from other child objects.             This method ensures that all child objects are properly validated.             The changes are only in memory and are not committed to the server.

### GetAllParentDocuments

Gets all parent documents, traversing the parent document chain by using the Parent property.              (Inherited from [Documents](General.Documents.md))  
_Return Type_: **Collection Of [Documents](General.Documents.md)**  
_Declaring Type_: **[Documents](General.Documents.md)**  
_Domain API Request_: **GET**  

**Parameters**  
  * **includeSelf**  
    if set to true the current document is included.  
    _Type_: boolean  
     _Optional_: True  
    _Default Value_: False  



## Business Rules

[!list erp.entity=Crm.Sales.SalesOrders erp.type=business-rule default-text=""None""]

## Front-End Business Rules

[!list erp.entity=Crm.Sales.SalesOrders erp.type=front-end-business-rule default-text=""None""]

## Generations

[!list erp.entity=Crm.Sales.SalesOrders erp.type=generation default-text=""None""]

## API

Domain API Query:
<https://demodb.my.erp.net/api/domain/odata/Crm_Sales_SalesOrders?$top=10>

Domain API Query Builder:
<https://demodb.my.erp.net/api/domain/querybuilder#Crm_Sales_SalesOrders?$top=10>

";
            #endregion
            var actualResult = ListOperatorParser.Parse(content);

            Assert.Empty(actualResult.Errors);
            Assert.Equal(3, actualResult.Lists.Length);

            // [!list erp.entity=Crm.Sales.SalesOrders erp.type=business-rule default-text=""None""]
            {
                var list = actualResult.Lists[0];

                Assert.Equal(2, list.Conditions.Count);
                Assert.Equal("None", list.DefaultText);
                Assert.Equal("Crm.Sales.SalesOrders", list.Conditions["erp.entity"]);
                Assert.Equal("business-rule", list.Conditions["erp.type"]);
            }

            // [!list erp.entity=Crm.Sales.SalesOrders erp.type=front-end-business-rule default-text=""None""]
            {
                var list = actualResult.Lists[1];

                Assert.Equal(2, list.Conditions.Count);
                Assert.Equal("None", list.DefaultText);
                Assert.Equal("Crm.Sales.SalesOrders", list.Conditions["erp.entity"]);
                Assert.Equal("front-end-business-rule", list.Conditions["erp.type"]);
            }

            // [!list erp.entity=Crm.Sales.SalesOrders erp.type=generation default-text=""None""]
            {
                var list = actualResult.Lists[2];

                Assert.Equal(2, list.Conditions.Count);
                Assert.Equal("None", list.DefaultText);
                Assert.Equal("Crm.Sales.SalesOrders", list.Conditions["erp.entity"]);
                Assert.Equal("generation", list.Conditions["erp.type"]);
            }
        }

        [Theory]
        [InlineData("[!list fruit=bananna]")]
        [InlineData("[!list \"fruit\"=bananna]")]
        [InlineData("[!list fruit=\"bananna\"]")]
        [InlineData("[!list \"fruit\"=\"bananna\"]")]
        public void CanParseAnyCombinationOfQuotedKeyValuePair(string input)
        {
            var result = ListOperatorParser.Parse(input);
            Assert.Empty(result.Errors);
            Assert.Single(result.Lists);
            
            var list = result.Lists[0];
            Assert.Single(list.Conditions);
            
            var cond = list.Conditions.First();
            Assert.Equal("fruit", cond.Key);
            Assert.Equal("bananna", cond.Value);
        }

        [Fact]
        public void CanParseEmptyList()
        {
            var r = ListOperatorParser.Parse(@"[!list]");

            Assert.Empty(r.Errors);
            Assert.Single(r.Lists);
        }

        [Fact]
        public void CanParseDefaultText()
        {
            var r = ListOperatorParser.Parse(@"# Operators

[!list items=Operators default-text=""None""]");

            Assert.Empty(r.Errors);
            Assert.Single(r.Lists);

            var list = r.Lists[0];
            Assert.Single(list.Conditions);

            Assert.Equal("None", list.DefaultText);

            var cond = list.Conditions.First();
            Assert.Equal("items", cond.Key);
            Assert.Equal("Operators", cond.Value);
        }

        [Fact]
        public void StartingAndEndingIndexAreCorrectlySet()
        {
            const string input = @"# Operators

[!list items=Operators default-text=""None""]
";

            int startingIndex = input.IndexOf("[!list ");
            int endingIndex = input.IndexOf("]", startingIndex + "[!list ".Length);

            var r = ListOperatorParser.Parse(input);

            Assert.Empty(r.Errors);

            var list = r.Lists[0];
            
            Assert.Equal(startingIndex, list.MatchedExpression.StartingIndex);
            Assert.Equal(endingIndex, list.MatchedExpression.EndingIndex);
        }

        [Theory]
        [InlineData("long-text-with-dash", "long-text-with-dash")]
        [InlineData("t23_asd", "t23_asd")]
        public void ReadWhileIdentifierCanHandleSpecialSymbols(string input, string output)
        {
            var context = ListOperatorParser.Context.Start(input);
            StringBuilder sink = new StringBuilder();
            var nextContext = ListOperatorParser.ReadWhileIdentifier(context, sink);

            Assert.Equal(output, sink.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData((string)null)]
        public void DoesntFailOnEmptyInput(string input)
        {
            var r = ListOperatorParser.Parse(input);

            Assert.Empty(r.Errors);
            Assert.Empty(r.Lists);
        }

        [Fact]
        public void CanParseMultipleLists()
        {
            var r = ListOperatorParser.Parse(@"abra cadabra [!list style=number file=oops]
some random text
more random text ad alsdlasdlasdl
[!list style=bullet limit=40 exclude=""dogecoin""]
adad a da dgjksd gglkasl f
sfskdflkd;lsfk
");

            Assert.Empty(r.Errors);
            Assert.Equal(2, r.Lists.Length);
        }
    }
}
