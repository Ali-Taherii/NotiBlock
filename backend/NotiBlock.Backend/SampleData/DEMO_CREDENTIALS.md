# NotiBlock Demo Credentials & Test Data

## 🔐 Universal Password
**All accounts use the same password for easy demo:**
```
Demo@123
```

---

## 👤 Consumer Accounts (5)

| Name | Email | Phone | Use Case |
|------|-------|-------|----------|
| John Smith | john.smith@email.com | +1-555-0101 | Has phone with active recall |
| Sarah Johnson | sarah.johnson@email.com | +1-555-0102 | Has tablet with pending recall |
| Michael Chen | michael.chen@email.com | +1-555-0103 | Has laptop with resolved recall |
| Emily Rodriguez | emily.rodriguez@email.com | +1-555-0104 | Has newer phone model |
| David Lee | david.lee@email.com | +1-555-0105 | Has smartwatch |

---

## 🏭 Manufacturer Accounts (3)

| Company Name | Email | Products |
|--------------|-------|----------|
| TechCorp Industries | contact@techcorp.com | SmartPhone X1, X2 models |
| SmartDevice Co | info@smartdevice.com | Tablet Pro, Watch |
| ElectroMax Ltd | support@electromax.com | Laptop Ultra |

---

## 🏪 Reseller Accounts (3)

| Company Name | Email | Description |
|--------------|-------|-------------|
| BestBuy Electronics | sales@bestbuy-demo.com | Large retail chain |
| TechMart Retail | contact@techmart-demo.com | Tech specialty store |
| Digital Zone Store | info@digitalzone-demo.com | Online & physical store |

---

## 🏛️ Regulator Accounts (2)

| Agency Name | Email | Role |
|-------------|-------|------|
| Consumer Product Safety Commission | cpsc@regulator-demo.gov | Recall approvals |
| Federal Trade Commission | ftc@regulator-demo.gov | Trade oversight |

---

## 📦 Sample Products (10)

### Owned by Consumers (5)
1. **TC-SMP-2024-001** - TechCorp SmartPhone X1 → John Smith (🔴 **Active Recall**)
2. **SD-TAB-2024-001** - SmartDevice Tablet Pro → Sarah Johnson (⚠️ **Pending Recall**)
3. **EM-LAP-2024-001** - ElectroMax Laptop Ultra → Michael Chen (✅ **Resolved Recall**)
4. **TC-SMP-2024-002** - TechCorp SmartPhone X2 → Emily Rodriguez
5. **SD-WTC-2024-001** - SmartDevice Watch → David Lee

### In Reseller Inventory (3)
6. **TC-SMP-2024-003** - TechCorp SmartPhone X1 @ BestBuy
7. **SD-TAB-2024-002** - SmartDevice Tablet Pro @ TechMart
8. **EM-LAP-2024-002** - ElectroMax Laptop Ultra @ Digital Zone

### Manufacturer Inventory (2)
9. **TC-SMP-2024-004** - TechCorp SmartPhone X2
10. **SD-WTC-2024-002** - SmartDevice Watch

---

## 🚨 Recalls (3)

### 1. Active Recall ✅ Approved
- **Product:** TC-SMP-2024-001 (TechCorp SmartPhone X1)
- **Reason:** Battery overheating - fire hazard risk
- **Action:** Return for battery replacement, don't charge
- **Status:** Active (Approved by CPSC)
- **Blockchain Hash:** 0x1a2b3c4d...

### 2. Pending Approval ⏳
- **Product:** SD-TAB-2024-001 (SmartDevice Tablet Pro)
- **Reason:** Software vulnerability - data access risk
- **Action:** Install update v2.5.1
- **Status:** Pending regulator approval

### 3. Resolved ✅
- **Product:** EM-LAP-2024-001 (ElectroMax Laptop Ultra)
- **Reason:** Defective power adapter - electric shock risk
- **Action:** Replace adapter
- **Status:** Resolved
- **Blockchain Hash:** 0x98765432...

---

## 📝 Consumer Reports (5)

1. **John Smith** - Screen flickering (Pending)
2. **Sarah Johnson** - Random reboots (Under Review)
3. **Michael Chen** - Keyboard not responding (✅ Resolved)
4. **Emily Rodriguez** - Battery drain (Pending)
5. **David Lee** - Sync issues (✅ Closed/Resolved)

---

## 🎯 Demo Scenarios

### Scenario 1: Consumer Experiencing Recall
- Login as: john.smith@email.com
- View active recall for TC-SMP-2024-001
- Check recall details and required actions
- See blockchain verification

### Scenario 2: Manufacturer Issuing Recall
- Login as: contact@techcorp.com
- Create new recall for product
- Track approval status
- Monitor affected users

### Scenario 3: Regulator Approval Workflow
- Login as: cpsc@regulator-demo.gov
- Review pending recall (SD-TAB-2024-001)
- Approve or reject with notes
- Publish to blockchain

### Scenario 4: Consumer Reporting Issue
- Login as: sarah.johnson@email.com
- Submit new product issue report
- Track report status
- View reseller responses

### Scenario 5: Reseller Managing Reports
- Login as: sales@bestbuy-demo.com
- View consumer reports
- Update report status
- Resolve issues

---

## 💾 How to Import Data

### Using pgAdmin:
1. Open pgAdmin and connect to your database
2. Right-click on your database → **Query Tool**
3. Open the `demo_data.sql` file
4. Click **Execute** (F5)
5. Verify the summary counts at the end

### Using psql command line:
```bash
psql -U your_username -d your_database -f demo_data.sql
```

---

## 🔄 Reset Data

To clean and re-import:
1. The SQL script includes DELETE statements at the top
2. Simply re-run the script to reset to fresh demo state
3. All existing data will be cleared first

---

## 📊 Quick Stats

- ✅ 5 Consumers ready for login
- ✅ 3 Manufacturers with products
- ✅ 3 Resellers with inventory
- ✅ 2 Regulatory agencies
- ✅ 10 Products across supply chain
- ✅ 3 Recalls (Active, Pending, Resolved)
- ✅ 5 Consumer reports (various statuses)

---

**Ready for your demo! 🚀**

All users authenticated and ready to test the full NotiBlock workflow from product registration to recall management and blockchain verification.
