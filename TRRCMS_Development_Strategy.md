# 🎯 TRRCMS Development Strategy - Incremental Build Plan

**Project:** UN-Habitat Tenure Rights Registration & Claims Management System  
**Approach:** Incremental Priority Groups  
**Timeline:** 3 Weeks to Core System  
**Current Status:** Building CRUD ✅ Complete

---

## 📊 Overview

**Total Entities:** 19  
**Strategy:** Build in batches of 2-3 entities, test thoroughly, then move to next batch  
**Pace:** 1 entity per day (or 2 days for complex ones)  

**Why Incremental?**
- ✅ Quick wins - Working system every 2-3 days
- ✅ Early testing - Catch problems before they multiply
- ✅ Logical flow - Build foundation first, then features
- ✅ Deployable milestones - Can demo after each batch
- ✅ Easy team integration - Clear entry points for new developers

---

## 🗓️ Week 1: Core Entities Foundation (Solo)

### **Batch 1: Property System** (Days 1-3)

**Status:** 🔨 In Progress

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| Building | P0 | - | ✅ Complete | Foundation pattern established |
| PropertyUnit | P0 | 1-2 days | 🔨 Next | Essential, directly related to Building |
| Survey | P1 | 1-2 days | ⏳ Pending | Connects Building + PropertyUnit + Person |

**Deliverable:** Working property registration system with building and unit management

---

### **Batch 2: People System** (Days 4-6)

**Status:** ⏳ Pending

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| Person | P0 | 1-2 days | ⏳ Pending | Core entity, needed for everything |
| Household | P1 | 1-2 days | ⏳ Pending | Groups people together |

**Deliverable:** Working people and household management system

**End of Week 1 Goal:** 5 entities complete (Building, PropertyUnit, Survey, Person, Household)

---

## 🗓️ Week 2: Relationships & Claims (Solo or +1 Team Member)

### **Batch 3: Core Relationships** (Days 7-9)

**Status:** ⏳ Pending

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| PersonPropertyRelation | P0 | 1-2 days | ⏳ Pending | Links people to properties (ownership) |
| BuildingAssignment | P1 | 1-2 days | ⏳ Pending | Links people to buildings (temporary) |

**Deliverable:** Complete property-ownership linkage system

**Team Split Option:**
- Developer 1: PersonPropertyRelation
- Developer 2: BuildingAssignment

---

### **Batch 4: Claims System** (Days 10-13)

**Status:** ⏳ Pending

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| Claim | P0 | 2 days | ⏳ Pending | Main workflow entity - most complex |
| Evidence | P1 | 1 day | ⏳ Pending | Supports claims with proof |
| Document | P1 | 1 day | ⏳ Pending | File attachments |

**Deliverable:** Basic claims submission and evidence attachment working

**Team Split Option:**
- Developer 1: Claim (more complex)
- Developer 2: Evidence + Document

**End of Week 2 Goal:** 10 entities complete, core workflow functional

---

## 🗓️ Week 3: Supporting Entities & Admin (With Team)

### **Batch 5: Workflow Support** (Days 14-17)

**Status:** ⏳ Pending

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| ConflictResolution | P1 | 1-2 days | ⏳ Pending | Handles disputed claims |
| Referrals | P2 | 1 day | ⏳ Pending | External referrals tracking |
| Certificate | P1 | 1 day | ⏳ Pending | Ownership certificates |

**Deliverable:** Complete claims workflow with conflict resolution

**Team Split:**
- Developer 1: ConflictResolution (complex business logic)
- Developer 2: Referrals + Certificate

---

### **Batch 6: System Administration** (Days 18-21)

**Status:** ⏳ Pending

| Entity | Priority | Estimated Time | Status | Notes |
|--------|----------|----------------|--------|-------|
| User | P0 | 1-2 days | ⏳ Pending | Authentication & authorization |
| AuditLog | P1 | 1 day | ⏳ Pending | Track all system changes |
| Vocabulary | P2 | 1 day | ⏳ Pending | Configurable dropdowns |
| ImportPackage | P2 | 1 day | ⏳ Pending | Bulk data import tracking |

**Deliverable:** Complete system with authentication and audit trail

**Team Split:**
- Developer 1: User + AuditLog (security-critical)
- Developer 2: Vocabulary + ImportPackage

**End of Week 3 Goal:** All 19 entities complete, full system functional

---

## 🔄 Daily Workflow (Per Entity)

### **Morning Session (2-3 hours)**

1. **Create branch:** `git checkout -b feature/entity-name-crud`
2. **Domain Layer:** Verify/update entity in TRRCMS.Domain
3. **Application Layer:**
   - Create DTO (EntityDto.cs)
   - Create Commands (CreateEntity, UpdateEntity, DeleteEntity)
   - Create Queries (GetEntity, GetAllEntities)
4. **Infrastructure Layer:**
   - Create Repository (EntityRepository.cs)
   - Create Configuration (EntityConfiguration.cs)
5. **Run Migration:** `Add-Migration Add{Entity}` and `Update-Database`

### **Afternoon Session (2-3 hours)**

6. **WebAPI Layer:**
   - Create Controller (EntitiesController.cs)
7. **Test in Swagger:**
   - POST: Create entity
   - GET: Retrieve entity
   - GET: List all entities
   - PUT: Update entity (if applicable)
   - DELETE: Soft delete (if applicable)
8. **Add Business Rules:** Validation, constraints, relationships
9. **Test Edge Cases:** Invalid data, missing fields, etc.
10. **Commit & Push:**
    ```bash
    git add .
    git commit -m "feat: Add {Entity} CRUD operations"
    git push origin feature/entity-name-crud
    ```

### **End of Day**

11. **Create Pull Request** on GitHub
12. **Self-review** or get team review
13. **Merge to main**

**Result:** 1 complete, tested entity per day

---

## 📋 Checklist Per Entity

### **Before Starting:**
- [ ] Read entity definition in Domain layer
- [ ] Understand relationships to other entities
- [ ] Review any special business rules
- [ ] Create feature branch

### **Development:**
- [ ] DTO created with all properties
- [ ] Create command & handler
- [ ] Update command & handler (if applicable)
- [ ] Get by ID query & handler
- [ ] Get all query & handler
- [ ] Repository interface & implementation
- [ ] EF Core configuration (table, columns, relationships)
- [ ] Migration created and applied
- [ ] Controller with all endpoints
- [ ] AutoMapper mapping configured

### **Testing:**
- [ ] Swagger UI accessible
- [ ] POST creates entity successfully
- [ ] GET retrieves entity by ID
- [ ] GET retrieves all entities
- [ ] PUT updates entity (if applicable)
- [ ] DELETE soft-deletes entity (if applicable)
- [ ] Validation works (required fields, formats)
- [ ] Relationships work (foreign keys, navigation)
- [ ] Data visible in PostgreSQL

### **Completion:**
- [ ] All endpoints tested
- [ ] No build errors
- [ ] No database errors
- [ ] Committed and pushed
- [ ] Pull request created
- [ ] Merged to main

---

## 🎯 Priority Levels Explained

**P0 (Critical):** Essential for MVP, blocking other entities
- Building ✅
- PropertyUnit
- Person
- Claim
- PersonPropertyRelation
- User

**P1 (High):** Important for core functionality
- Household
- Survey
- Evidence
- Document
- ConflictResolution
- Certificate
- AuditLog

**P2 (Medium):** Supporting features, can be done later
- BuildingAssignment
- Referrals
- Vocabulary
- ImportPackage

---

## 📊 Progress Tracking

### **Week 1 Target: 5 Entities**
- [x] Building (Day 0 - Complete)
- [ ] PropertyUnit (Day 1-2)
- [ ] Survey (Day 3)
- [ ] Person (Day 4-5)
- [ ] Household (Day 6)

### **Week 2 Target: +5 Entities (Total: 10)**
- [ ] PersonPropertyRelation (Day 7-8)
- [ ] BuildingAssignment (Day 9)
- [ ] Claim (Day 10-11)
- [ ] Evidence (Day 12)
- [ ] Document (Day 13)

### **Week 3 Target: +9 Entities (Total: 19)**
- [ ] ConflictResolution (Day 14-15)
- [ ] Referrals (Day 16)
- [ ] Certificate (Day 17)
- [ ] User (Day 18-19)
- [ ] AuditLog (Day 20)
- [ ] Vocabulary (Day 21)
- [ ] ImportPackage (Day 21)

---

## 🚀 Next Actions

### **Immediate (Today):**
1. ✅ Review this plan
2. 🔨 Start PropertyUnit entity
3. 🔨 Create feature branch: `feature/property-unit-crud`
4. 🔨 Follow the daily workflow checklist

### **This Week:**
- Complete PropertyUnit, Survey, Person, Household
- Commit and push after each entity
- Test thoroughly in Swagger
- Keep PostgreSQL data clean (delete test data between entities)

### **Next Week:**
- Onboard team member if available
- Split work on relationships and claims
- Set up code review process
- Start thinking about authentication

---

## 💡 Tips for Success

### **Development:**
- **Start simple:** Basic CRUD first, complex logic later
- **Test frequently:** After every small change
- **Commit often:** Small, focused commits with clear messages
- **Follow the pattern:** Building entity is your template
- **Use the guides:** Reference TRRCMS_HowToExtend.md

### **When Stuck:**
- **Check Building implementation:** It's your working reference
- **Review Domain entity:** Make sure you understand the properties
- **Test database:** Use pgAdmin to verify data structure
- **Break it down:** Focus on one operation at a time (Create, then Read, etc.)

### **Team Coordination:**
- **Communicate:** Daily standup or chat about progress
- **Different branches:** Each developer works on separate entity
- **Pull before push:** `git pull origin main` before pushing
- **Review each other:** Even quick reviews catch mistakes
- **Share blockers:** Don't get stuck alone for hours

---

## 🎉 Success Criteria

### **Week 1 Success:**
✅ 5 entities fully functional  
✅ All CRUD operations tested  
✅ Data persisting in PostgreSQL  
✅ No critical bugs  
✅ Code committed to GitHub  

### **Week 2 Success:**
✅ 10 entities total  
✅ Core workflow (Create claim, add evidence) working  
✅ Relationships between entities functional  
✅ Team member onboarded (if available)  

### **Week 3 Success:**
✅ All 19 entities complete  
✅ Authentication working  
✅ Full claim workflow end-to-end  
✅ System ready for advanced features  
✅ Documentation complete  

---

## 📅 Timeline Summary

| Week | Days | Focus | Entities | Total | Deliverable |
|------|------|-------|----------|-------|-------------|
| 1 | 1-6 | Foundation | 5 | 5 | Property & People System |
| 2 | 7-13 | Relationships & Claims | 5 | 10 | Core Workflow |
| 3 | 14-21 | Support & Admin | 9 | 19 | Complete System |

**Total Time:** 3 weeks  
**Final Result:** Production-ready backend API with all 19 entities

---

**Last Updated:** January 4, 2026  
**Status:** Week 1 - Batch 1 - PropertyUnit (Next)

---

## 📞 Resources

- **How to Extend Guide:** [TRRCMS_HowToExtend.md](./docs/TRRCMS_HowToExtend.md)
- **Setup Guide:** [SETUP_GUIDE.md](./SETUP_GUIDE.md)
- **Full Analysis:** [TRRCMS_Analysis_NextSteps.md](./docs/TRRCMS_Analysis_NextSteps.md)
- **GitHub Repo:** https://github.com/Ameerovich/TRRCMS-Backend

---

**Ready to build! Let's start with PropertyUnit! 🚀**
