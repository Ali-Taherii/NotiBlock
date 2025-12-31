using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotiBlock.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddResellerTicketReadableView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW vw_ResellerTickets_Readable AS
                SELECT 
                    ""Id"",
                    ""ResellerId"",
                    
                    -- Category (readable)
                    CASE ""Category""
                        WHEN 0 THEN 'Product Defect'
                        WHEN 1 THEN 'Quality Issue'
                        WHEN 2 THEN 'Safety Concern'
                        WHEN 3 THEN 'Counterfeit Suspicion'
                        WHEN 4 THEN 'Supply Chain Issue'
                        WHEN 5 THEN 'Customer Complaint'
                        WHEN 6 THEN 'Other'
                        ELSE 'Unknown'
                    END as ""CategoryText"",
                    ""Category"" as ""CategoryCode"",
                    
                    ""Description"",
                    
                    -- Status (readable)
                    CASE ""Status""
                        WHEN 0 THEN 'Pending'
                        WHEN 1 THEN 'Under Review'
                        WHEN 2 THEN 'Approved'
                        WHEN 3 THEN 'Rejected'
                        WHEN 4 THEN 'Resolved'
                        WHEN 5 THEN 'Closed'
                        ELSE 'Unknown'
                    END as ""StatusText"",
                    ""Status"" as ""StatusCode"",
                    
                    -- Priority (readable)
                    CASE ""Priority""
                        WHEN 0 THEN 'Low'
                        WHEN 1 THEN 'Medium'
                        WHEN 2 THEN 'High'
                        WHEN 3 THEN 'Critical'
                        ELSE 'Unknown'
                    END as ""PriorityText"",
                    ""Priority"" as ""PriorityCode"",
                    
                    ""CreatedAt"",
                    ""UpdatedAt"",
                    ""ResolvedAt"",
                    ""ApprovedById"",
                    ""ResolvedBy"",
                    ""ResolutionNotes"",
                    ""IsDeleted"",
                    ""DeletedAt"",
                    ""DeletedBy""
                FROM ""ResellerTickets""
                WHERE ""IsDeleted"" = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_ResellerTickets_Readable;");
        }
    }
}